import os
import logging
from typing import List, Dict, Any, Optional
import json
import inspect

from fastapi import FastAPI, HTTPException, Body
from pydantic import BaseModel
import google.generativeai as genai
from google.generativeai.types import HarmCategory, HarmBlockThreshold

from dotenv import load_dotenv

from tools.finance_tools import AVAILABLE_TOOLS as FINANCE_TOOLS, FUNCTION_MAPPING as FINANCE_FUNCTION_MAPPING
from tools.membership_tools import MEMBERSHIP_AVAILABLE_TOOLS, MEMBERSHIP_FUNCTION_MAPPING
from tools.budget_tools import BUDGET_AVAILABLE_TOOLS, BUDGET_FUNCTION_MAPPING
from tools.account_tools import ACCOUNT_AVAILABLE_TOOLS, ACCOUNT_FUNCTION_MAPPING

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

load_dotenv()

GOOGLE_API_KEY = os.getenv("GOOGLE_API_KEY")
if not GOOGLE_API_KEY:
    logger.error("GOOGLE_API_KEY ortam değişkeni bulunamadı!")
    raise ValueError("GOOGLE_API_KEY ortam değişkeni ayarlanmalıdır.")

genai.configure(api_key=GOOGLE_API_KEY)

app = FastAPI(title="FinTrack ChatBot Service (Python)")

chat_histories: Dict[str, List[Dict[str, Any]]] = {}

class ChatRequest(BaseModel):
    userId: str
    clientChatSessionId: str
    message: str
    authToken: Optional[str] = None

class ChatResponse(BaseModel):
    reply: str

ALL_AVAILABLE_TOOLS = FINANCE_TOOLS + MEMBERSHIP_AVAILABLE_TOOLS + BUDGET_AVAILABLE_TOOLS + ACCOUNT_AVAILABLE_TOOLS
ALL_FUNCTION_MAPPING = {**FINANCE_FUNCTION_MAPPING, **MEMBERSHIP_FUNCTION_MAPPING, **BUDGET_FUNCTION_MAPPING, **ACCOUNT_FUNCTION_MAPPING}

gemini_tools_config: Optional[List[genai.types.Tool]] = None
if ALL_AVAILABLE_TOOLS:
    try:
        gemini_tools_config = [genai.types.Tool(function_declarations=[
            genai.types.FunctionDeclaration(**tool_schema) for tool_schema in ALL_AVAILABLE_TOOLS
        ])]
    except Exception as e:
        logger.error(f"Gemini araçları oluşturulurken hata: {e}")

def get_system_prompt(user_id: str, tools_config: Optional[List[genai.types.Tool]]) -> str:
    tool_descriptions = []
    if tools_config:
        for tool_wrapper in tools_config:
            for func_decl in tool_wrapper.function_declarations:
                tool_descriptions.append(f"- '{func_decl.name}': {func_decl.description}")
    
    available_tools_str = "\n".join(tool_descriptions)
    if not tool_descriptions:
        available_tools_str = "Şu anda özel bir finansal veya üyelik aracım bulunmuyor."

    return f"""Sen FinBot'sun. FinTrack kullanıcısı (UserId: {user_id}) için bir finans, üyelik, bütçe ve hesap yönetimi asistanısın.
    Amacın, kullanıcılara finansal konularda, üyelik durumlarıyla, bütçeleriyle ve hesaplarıyla ilgili yardımcı olmaktır.
    Kullanılabilir Araçların (Fonksiyonların):
    {available_tools_str} # Bu artık tüm fonksiyonları içerecek
    Kullanıcı bir istekte bulunduğunda, eğer uygun bir araç varsa, o aracı çağırmak için bir 'function_call' isteği döndür.
    Örneğin:
    - Kullanıcı 'tüm hesaplarımı listele' veya 'hangi banka hesaplarım var?' derse, 'get_user_accounts' fonksiyonunu çağır.
    - Kullanıcı 'maaş hesabımın bakiyesi nedir?' veya 'ID'si 3 olan hesabımın detaylarını göster' derse, 'get_account_details' fonksiyonunu uygun 'account_id' ile çağır (gerekirse ID'yi kullanıcıdan iste).
    - Kullanıcı 'yeni bir nakit cüzdanı oluşturmak istiyorum, adı Cep Harçlığı olsun, başlangıç bakiyesi 100 TL' derse, 'create_account' fonksiyonunu kullanmak için gerekli bilgileri (isim, tür, bakiye) çıkarım yap veya kullanıcıdan iste.
    Dönen verileri kullanıcıya anlaşılır bir şekilde özetleyerek sun. Eğer fonksiyon bir hata döndürürse veya veri bulamazsa, bunu uygun bir şekilde kullanıcıya bildir.
    Eğer bir fonksiyonu çağırmak için gerekli bilgi eksikse, bu bilgiyi kullanıcıdan İSTE.
    Yanıtlarında ASLA '(Bu kısımda ... fonksiyonu çağrılır)' gibi parantez içi açıklamalar KULLANMA; fonksiyonu gerçekten çağır ve sonucunu doğrudan ilet.
    Her zaman nazik ol. Finansal tavsiye verme. Karmaşık durumlar için uzmana yönlendir.
    Projenin adı FinTrack Finans Takip Uygulamasıdır."""

@app.post("/chat", response_model=ChatResponse)
async def chat_endpoint(request: ChatRequest = Body(...)):
    logger.info(f"Python: /chat endpoint'ine istek geldi: UserId={request.userId}, SessionId={request.clientChatSessionId}, Message='{request.message}', HasAuthToken={request.authToken is not None}")

    session_cache_key = f"history_{request.userId}_{request.clientChatSessionId}"
    current_history: List[Dict[str, Any]] = chat_histories.get(session_cache_key, [])
    
    if not current_history:
        logger.info(f"Python: Yeni sohbet geçmişi için başlangıç yapılıyor. CacheKey: {session_cache_key}")
    
    current_history.append({"role": "user", "parts": [{"text": request.message}]})
    
    final_reply_text: str = "Üzgünüm, isteğinizi işlerken bir sorunla karşılaştım."

    try:
        model = genai.GenerativeModel(
            model_name="gemini-1.5-flash-latest",
            tools=gemini_tools_config,
            system_instruction=get_system_prompt(request.userId, gemini_tools_config),
            safety_settings={
                HarmCategory.HARM_CATEGORY_HARASSMENT: HarmBlockThreshold.BLOCK_NONE,
                HarmCategory.HARM_CATEGORY_HATE_SPEECH: HarmBlockThreshold.BLOCK_NONE,
                HarmCategory.HARM_CATEGORY_SEXUALLY_EXPLICIT: HarmBlockThreshold.BLOCK_NONE,
                HarmCategory.HARM_CATEGORY_DANGEROUS_CONTENT: HarmBlockThreshold.BLOCK_NONE,
            }
        )
        
        for i in range(2): 
            logger.info(f"Python: Gemini'ye istek gönderiliyor (Tur {i+1}). History Length: {len(current_history)}")
            
            response_from_gemini = await model.generate_content_async(
                contents=current_history,
                generation_config={"candidate_count": 1}
            )

            try:
                logger.debug(f"Python: Gemini Raw Response Object (Tur {i+1}): {response_from_gemini}")
                if response_from_gemini.prompt_feedback and response_from_gemini.prompt_feedback.block_reason:
                    logger.warning(f"Python: Yanıt engellendi (Tur {i+1}). Neden: {response_from_gemini.prompt_feedback.block_reason_message}")
                    final_reply_text = f"İsteğiniz güvenlik filtreleri nedeniyle işlenemedi: {response_from_gemini.prompt_feedback.block_reason_message}"
                    current_history.append({"role": "model", "parts": [{"text": final_reply_text}]})
                    break 
            except Exception as log_ex:
                logger.warning(f"Python: Gemini yanıtını loglarken/kontrol ederken hata (Tur {i+1}): {log_ex}")

            if not response_from_gemini.candidates:
                logger.warning(f"Python: Gemini'den aday yanıt gelmedi (Tur {i+1}).")
                final_reply_text = "Gemini modelinden yanıt alınamadı."
                current_history.append({"role": "model", "parts": [{"text": final_reply_text}]})
                break

            ai_candidate = response_from_gemini.candidates[0]
            ai_message_content = ai_candidate.content
            
            ai_message_for_history: Dict[str, Any]
            if hasattr(ai_message_content, 'to_dict'):
                ai_message_for_history = ai_message_content.to_dict()
            else:
                ai_message_for_history = {"role": ai_message_content.role, "parts": []}
                for p_val in ai_message_content.parts:
                    part_dict = {}
                    if hasattr(p_val, 'text') and p_val.text:
                        part_dict["text"] = p_val.text
                    elif hasattr(p_val, 'function_call') and p_val.function_call:
                        part_dict["function_call"] = {"name": p_val.function_call.name, "args": dict(p_val.function_call.args) if p_val.function_call.args else {}}
                    if part_dict:
                         ai_message_for_history["parts"].append(part_dict)

            if not ai_message_content.parts:
                logger.warning(f"Python: Gemini yanıtında 'parts' bulunamadı (Tur {i+1}).")
                final_reply_text = "Gemini modelinden içeriksiz yanıt alındı."
                current_history.append({"role": "model", "parts": [{"text": final_reply_text}]})
                break
            
            called_function_in_this_turn = False
            for part_content in ai_message_content.parts:
                if hasattr(part_content, 'function_call') and part_content.function_call:
                    called_function_in_this_turn = True
                    function_call_data = part_content.function_call 
                    function_name = function_call_data.name
                    args = dict(function_call_data.args) if function_call_data.args else {}
                    
                    logger.info(f"Python: Gemini fonksiyon çağırmayı önerdi (Tur {i+1}): {function_name} with args: {args}")
                    current_history.append(ai_message_for_history)

                    if function_name in ALL_FUNCTION_MAPPING:
                        python_function_to_call = ALL_FUNCTION_MAPPING[function_name]
                        
                        func_params = inspect.signature(python_function_to_call).parameters
                        if "auth_token" in func_params: 
                            if request.authToken:
                                args["auth_token"] = request.authToken 
                                logger.info(f"Auth token eklendi: {function_name} fonksiyonu için.")
                            else:
                                logger.error(f"Auth Token eksik! {function_name} çağrılamıyor.")
                                final_reply_text = "Bu işlemi yapmak için kimlik doğrulama bilgisi (authToken) gerekli ve sağlanmadı."
                                error_tool_response_for_history = {
                                    "role": "tool",
                                    "parts": [{"function_response": {"name": function_name, "response": {"error": "Auth token eksik"}}}]
                                }
                                current_history.append(error_tool_response_for_history)
                                break 
                        
                        if "auth_token" not in args and "auth_token" in func_params:
                             logger.error(f"Auth token hala eksik, fonksiyon çağrılmayacak: {function_name}")
                             final_reply_text = "Kimlik doğrulama hatası nedeniyle işlem yapılamadı."
                             break

                        try:
                            function_response_data = python_function_to_call(**args)
                            logger.info(f"Python: Fonksiyon '{function_name}' çalıştırıldı, sonuç tipi: {type(function_response_data)}")
                            tool_response_for_history = {
                                "role": "tool",
                                "parts": [{"function_response": {"name": function_name, "response": {"result": function_response_data}}}]
                            }
                            current_history.append(tool_response_for_history)
                            final_reply_text = "Fonksiyon çalıştırıldı, sonuç Gemini'ye gönderiliyor..." 
                        except TypeError as te:
                            logger.error(f"Python: Fonksiyon '{function_name}' çağrılırken TypeError: {te}", exc_info=True)
                            final_reply_text = f"'{function_name}' aracını çağırırken bir argüman hatası oluştu: {str(te)}"
                            error_tool_response_for_history = {
                                "role": "tool",
                                "parts": [{"function_response": {"name": function_name, "response": {"error": f"Argüman hatası: {str(te)}" }}}]
                            }
                            current_history.append(error_tool_response_for_history)
                            break 
                        except Exception as e:
                            logger.error(f"Python: Fonksiyon '{function_name}' çalıştırılırken hata: {e}", exc_info=True)
                            final_reply_text = f"'{function_name}' aracını kullanırken bir sorunla karşılaştım: {str(e)}"
                            error_tool_response_for_history = {
                                "role": "tool",
                                "parts": [{"function_response": {"name": function_name, "response": {"error": str(e)}}}]
                            }
                            current_history.append(error_tool_response_for_history)
                            break 
                    else:
                        logger.warning(f"Python: Gemini bilinmeyen bir fonksiyon çağırmayı önerdi (Tur {i+1}): {function_name}")
                        final_reply_text = "İsteğinizi anladım ancak uygun bir araç bulamadım."
                    break 
            
            if called_function_in_this_turn:
                if i == 0: 
                    continue 
                else: 
                    logger.warning(f"Python: Fonksiyon çağrısı sonrası ikinci turda metin yanıtı bekleniyordu (Tur {i+1}).")
                    text_part_found = False
                    for part_content_after_func in ai_message_content.parts:
                        if hasattr(part_content_after_func, 'text') and part_content_after_func.text:
                            final_reply_text = part_content_after_func.text
                            text_part_found = True
                            break
                    if not text_part_found:
                        final_reply_text = "Fonksiyon işlendi ancak son bir yanıt alınamadı."
                    break

            text_part_found_direct = False
            for part_content_direct in ai_message_content.parts:
                if hasattr(part_content_direct, 'text') and part_content_direct.text:
                    final_reply_text = part_content_direct.text
                    text_part_found_direct = True
                    logger.info(f"Python: Gemini direkt metin yanıtı verdi (Tur {i+1}): {final_reply_text}")
                    break 
            
            if text_part_found_direct:
                break 
            elif not called_function_in_this_turn : 
                logger.warning(f"Python: Gemini'den beklenmedik yanıt formatı (ne fonksiyon ne metin) (Tur {i+1}).")
                break 
        
        chat_histories[session_cache_key] = current_history
        
        if not current_history or \
           not (current_history[-1].get("role") == "model" and \
                len(current_history[-1].get("parts", [])) > 0 and \
                current_history[-1]["parts"][0].get("text") == final_reply_text):
            current_history.append({"role": "model", "parts": [{"text": final_reply_text}]})

        if len(current_history) > 20: 
            chat_histories[session_cache_key] = current_history[-20:]

        return ChatResponse(reply=final_reply_text)

    except Exception as e:
        logger.error(f"Python: /chat endpoint'inde genel bir hata oluştu: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"ChatBot servisinde bir hata oluştu: {str(e)}")

if __name__ == "__main__":
    import uvicorn
    logger.info("Python ChatBot servisi başlatılıyor...")
    uvicorn.run(app, host="0.0.0.0", port=8000)