# -*- coding: windows-1254 -*-

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

from Tools.FinanceTools import AVAILABLE_TOOLS as FINANCE_TOOLS, FUNCTION_MAPPING as FINANCE_FUNCTION_MAPPING
from Tools.MembershipTools import MEMBERSHIP_AVAILABLE_TOOLS, MEMBERSHIP_FUNCTION_MAPPING
from Tools.BudgetTools import BUDGET_AVAILABLE_TOOLS, BUDGET_FUNCTION_MAPPING
from Tools.AccountTools import ACCOUNT_AVAILABLE_TOOLS, ACCOUNT_FUNCTION_MAPPING
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

load_dotenv()

GOOGLE_API_KEY = os.getenv("GOOGLE_API_KEY")
if not GOOGLE_API_KEY:
    logger.error("GOOGLE_API_KEY environment variable not found!")
    raise ValueError("GOOGLE_API_KEY environment variable must be set.")

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
        logger.error(f"Error creating Gemini tools: {e}")

def get_system_prompt(user_id: str, tools_config: Optional[List[genai.types.Tool]]) -> str:
    tool_descriptions = []
    if tools_config:
        for tool_wrapper in tools_config:
            for func_decl in tool_wrapper.function_declarations:
                tool_descriptions.append(f"- '{func_decl.name}': {func_decl.description}")
    
    available_tools_str = "\n".join(tool_descriptions)
    if not tool_descriptions:
        available_tools_str = "�u anda �zel bir finansal veya �yelik arac�m bulunmuyor."

    return f"""Sen FinBot'sun. FinTrack kullan�c�s� (UserId: {user_id}) i�in bir finans, �yelik, b�t�e ve hesap y�netimi asistan�s�n.
    Amac�n, kullan�c�lara finansal konularda, �yelik durumlar�yla, b�t�eleriyle ve hesaplar�yla ilgili yard�mc� olmakt�r.
    Kullan�labilir Ara�lar�n (Fonksiyonlar�n):
    {available_tools_str} # Bu art�k t�m fonksiyonlar� i�erecek
    Kullan�c� bir istekte bulundu�unda, e�er uygun bir ara� varsa, o arac� �a��rmak i�in bir 'function_call' iste�i d�nd�r.
    �rne�in:
    - Kullan�c� 't�m hesaplar�m� listele' veya 'hangi banka hesaplar�m var?' derse, 'get_user_accounts' fonksiyonunu �a��r.
    - Kullan�c� 'maa� hesab�m�n bakiyesi nedir?' veya 'ID'si 3 olan hesab�m�n detaylar�n� g�ster' derse, 'get_account_details' fonksiyonunu uygun 'account_id' ile �a��r (gerekirse ID'yi kullan�c�dan iste).
    - Kullan�c� 'yeni bir nakit c�zdan� olu�turmak istiyorum, ad� Cep Har�l��� olsun, ba�lang�� bakiyesi 100 TL' derse, 'create_account' fonksiyonunu kullanmak i�in gerekli bilgileri (isim, t�r, bakiye) ��kar�m yap veya kullan�c�dan iste.
    D�nen verileri kullan�c�ya anla��l�r bir �ekilde �zetleyerek sun. E�er fonksiyon bir hata d�nd�r�rse veya veri bulamazsa, bunu uygun bir �ekilde kullan�c�ya bildir.
    E�er bir fonksiyonu �a��rmak i�in gerekli bilgi eksikse, bu bilgiyi kullan�c�dan �STE.
    Yan�tlar�nda ASLA '(Bu k�s�mda ... fonksiyonu �a�r�l�r)' gibi parantez i�i a��klamalar KULLANMA; fonksiyonu ger�ekten �a��r ve sonucunu do�rudan ilet.
    Her zaman nazik ol. Finansal tavsiye verme. Karma��k durumlar i�in uzmana y�nlendir.
    Projenin ad� FinTrack Finans Takip Uygulamas�d�r."""

@app.post("/chat", response_model=ChatResponse)
async def chat_endpoint(request: ChatRequest = Body(...)):
    logger.info(f"Python: Request received at /chat endpoint: UserId={request.userId}, SessionId={request.clientChatSessionId}, Message='{request.message}', HasAuthToken={request.authToken is not None}")

    session_cache_key = f"history_{request.userId}_{request.clientChatSessionId}"
    current_history: List[Dict[str, Any]] = chat_histories.get(session_cache_key, [])
    
    if not current_history:
        logger.info(f"Python: Starting new chat history. CacheKey: {session_cache_key}")
    
    current_history.append({"role": "user", "parts": [{"text": request.message}]})
    
    final_reply_text: str = "Sorry, I encountered a problem while processing your request."

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
            logger.info(f"Python: Request sent to Gemini (Round {i+1}). History Length: {len(current_history)}")
            
            response_from_gemini = await model.generate_content_async(
                contents=current_history,
                generation_config={"candidate_count": 1}
            )

            try:
                logger.debug(f"Python: Gemini Raw Response Object (Round {i+1}): {response_from_gemini}")
                if response_from_gemini.prompt_feedback and response_from_gemini.prompt_feedback.block_reason:
                    logger.warning(f"Python: Response blocked (Round {i+1}). Reason: {response_from_gemini.prompt_feedback.block_reason_message}")
                    final_reply_text = f"Your request could not be processed due to safety filters: {response_from_gemini.prompt_feedback.block_reason_message}"
                    current_history.append({"role": "model", "parts": [{"text": final_reply_text}]})
                    break 
            except Exception as log_ex:
                logger.warning(f"Python: Error while logging/checking Gemini response (Round {i+1}): {log_ex}")

            if not response_from_gemini.candidates:
                logger.warning(f"Python: No candidate response received from Gemini (Round {i+1}).")
                final_reply_text = "No response received from the Gemini model."
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
                logger.warning(f"Python: 'parts' not found in Gemini response (Round {i+1}).")
                final_reply_text = "Received an empty response from the Gemini model."
                current_history.append({"role": "model", "parts": [{"text": final_reply_text}]})
                break
            
            called_function_in_this_turn = False
            for part_content in ai_message_content.parts:
                if hasattr(part_content, 'function_call') and part_content.function_call:
                    called_function_in_this_turn = True
                    function_call_data = part_content.function_call 
                    function_name = function_call_data.name
                    args = dict(function_call_data.args) if function_call_data.args else {}
                    
                    logger.info(f"Python: Gemini suggested a function call (Round {i+1}): {function_name} with args: {args}")
                    current_history.append(ai_message_for_history)

                    if function_name in ALL_FUNCTION_MAPPING:
                        python_function_to_call = ALL_FUNCTION_MAPPING[function_name]
                        
                        func_params = inspect.signature(python_function_to_call).parameters
                        if "auth_token" in func_params: 
                            if request.authToken:
                                args["auth_token"] = request.authToken 
                                logger.info(f"Auth token added: For function {function_name}.")
                            else:
                                logger.error(f"Auth Token missing! Cannot call {function_name}.")
                                final_reply_text = "Authentication information (authToken) is required and was not provided for this operation."
                                error_tool_response_for_history = {
                                    "role": "tool",
                                    "parts": [{"function_response": {"name": function_name, "response": {"error": "Auth token eksik"}}}]
                                }
                                current_history.append(error_tool_response_for_history)
                                break 
                        
                        if "auth_token" not in args and "auth_token" in func_params:
                             logger.error(f"Auth token still missing, function will not be called: {function_name}")
                             final_reply_text = "Operation could not be performed due to authentication error."
                             break

                        try:
                            function_response_data = python_function_to_call(**args)
                            logger.info(f"Python: Function '{function_name}' executed, result type: {type(function_response_data)}")
                            tool_response_for_history = {
                                "role": "tool",
                                "parts": [{"function_response": {"name": function_name, "response": {"result": function_response_data}}}]
                            }
                            current_history.append(tool_response_for_history)
                            final_reply_text = "Function executed, result is being sent to Gemini..." 
                        except TypeError as te:
                            logger.error(f"Python: TypeError while calling function '{function_name}': {te}", exc_info=True)
                            final_reply_text = f"An argument error occurred while calling the '{function_name}' tool: {str(te)}"
                            error_tool_response_for_history = {
                                "role": "tool",
                                "parts": [{"function_response": {"name": function_name, "response": {"error": f"Arg�man hatas�: {str(te)}" }}}]
                            }
                            current_history.append(error_tool_response_for_history)
                            break 
                        except Exception as e:
                            logger.error(f"Python: Error while executing function '{function_name}': {e}", exc_info=True)
                            final_reply_text = f"An issue occurred while using the '{function_name}' tool: {str(e)}"
                            error_tool_response_for_history = {
                                "role": "tool",
                                "parts": [{"function_response": {"name": function_name, "response": {"error": str(e)}}}]
                            }
                            current_history.append(error_tool_response_for_history)
                            break 
                    else:
                        logger.warning(f"Python: Gemini suggested an unknown function call (Round {i+1}): {function_name}")
                        final_reply_text = "I understood your request but could not find a suitable tool."
                    break 
            
            if called_function_in_this_turn:
                if i == 0: 
                    continue 
                else: 
                    logger.warning(f"Python: Expected text response in the second round after function call (Round {i+1}).")
                    text_part_found = False
                    for part_content_after_func in ai_message_content.parts:
                        if hasattr(part_content_after_func, 'text') and part_content_after_func.text:
                            final_reply_text = part_content_after_func.text
                            text_part_found = True
                            break
                    if not text_part_found:
                        final_reply_text = "Function processed but no final response was received."
                    break

            text_part_found_direct = False
            for part_content_direct in ai_message_content.parts:
                if hasattr(part_content_direct, 'text') and part_content_direct.text:
                    final_reply_text = part_content_direct.text
                    text_part_found_direct = True
                    logger.info(f"Python: Gemini provided a direct text response (Round {i+1}): {final_reply_text}")
                    break 
            
            if text_part_found_direct:
                break 
            elif not called_function_in_this_turn : 
                logger.warning(f"Python: Unexpected response format from Gemini (neither function nor text) (Round {i+1}).")
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
        logger.error(f"Python: General error occurred in /chat endpoint: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"An error occurred in the ChatBot service: {str(e)}")

if __name__ == "__main__":
    import uvicorn
    logger.info("Python ChatBot service is starting...")
    uvicorn.run(app, host="0.0.0.0", port=8000)