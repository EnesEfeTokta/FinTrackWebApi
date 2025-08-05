import os
import logging
import json
import inspect
from typing import List, Dict, Any, Optional

import requests
from fastapi import FastAPI, HTTPException, Body, Request
from fastapi.responses import PlainTextResponse
# DEĞİŞİKLİK: 'pantic' yazım hatası 'pydantic' olarak düzeltildi.
from pydantic import BaseModel, Field
from dotenv import load_dotenv
from prometheus_client import generate_latest, Counter, Histogram
import time
from datetime import datetime, timezone

# --- Modül ve Araç Import'ları ---
from Tools.TransactionTools import TRANSACTION_AVAILABLE_TOOLS, TRANSACTION_FUNCTION_MAPPING
from Tools.MembershipTools import MEMBERSHIP_AVAILABLE_TOOLS, MEMBERSHIP_FUNCTION_MAPPING
from Tools.BudgetTools import BUDGET_AVAILABLE_TOOLS, BUDGET_FUNCTION_MAPPING
from Tools.AccountTools import ACCOUNT_AVAILABLE_TOOLS, ACCOUNT_FUNCTION_MAPPING
from Tools.CalculatorTools import CALCULATOR_AVAILABLE_TOOLS, CALCULATOR_FUNCTION_MAPPING
from Tools._api_helpers import DecimalEncoder 

# --- Loglama Kurulumu ---
# Log formatını daha detaylı hale getirelim
LOG_FORMAT = '%(asctime)s - %(levelname)s - [%(filename)s:%(lineno)d] - %(message)s'
logging.basicConfig(level=logging.INFO, format=LOG_FORMAT)
logger = logging.getLogger(__name__)

# --- Ortam Değişkenleri ve FastAPI Kurulumu ---
load_dotenv()
OLLAMA_API_URL = os.getenv("OLLAMA_API_URL", "http://localhost:11434") 
OLLAMA_MODEL_NAME = "phi3:mini"
app = FastAPI(title="FinTrack ChatBot Service (Python with Ollama)")

# --- Prometheus Metrikleri ---
REQUEST_COUNT = Counter('finbot_http_requests_total', 'Total HTTP requests', ['method', 'endpoint', 'status_code'])
REQUEST_LATENCY = Histogram('finbot_http_request_duration_seconds', 'HTTP request latency', ['endpoint'])
MESSAGES_PROCESSED_TOTAL = Counter('finbot_messages_processed_total', 'Total messages processed')
FINBOT_RESPONSE_TIME = Histogram('finbot_response_duration_seconds', 'FinBot response duration')

# --- Middleware ---
@app.middleware("http")
async def add_process_time_header(request: Request, call_next):
    start_time = time.time()
    response = await call_next(request)
    process_time = time.time() - start_time
    endpoint = request.url.path
    method = request.method
    status_code = response.status_code
    REQUEST_COUNT.labels(method=method, endpoint=endpoint, status_code=status_code).inc()
    REQUEST_LATENCY.labels(endpoint=endpoint).observe(process_time)
    return response

# --- Pydantic Modelleri ---
class ChatMessage(BaseModel):
    role: str
    content: str

class ChatRequest(BaseModel):
    userId: str
    clientChatSessionId: str
    message: str
    authToken: Optional[str] = None
    history: Optional[List[ChatMessage]] = Field(default_factory=list)

class ChatResponse(BaseModel):
    reply: str
    responseTime: datetime 

# --- Güvenli Araç Birleştirme ---
def merge_tool_mappings(*mappings: Dict[str, callable]) -> Dict[str, callable]:
    merged = {}
    for mapping in mappings:
        for key, value in mapping.items():
            if key in merged:
                raise NameError(f"Duplicate function name '{key}' found in tool mappings.")
            merged[key] = value
    return merged

ALL_AVAILABLE_TOOLS = (
    TRANSACTION_AVAILABLE_TOOLS + MEMBERSHIP_AVAILABLE_TOOLS + 
    BUDGET_AVAILABLE_TOOLS + ACCOUNT_AVAILABLE_TOOLS + CALCULATOR_AVAILABLE_TOOLS
)
try:
    ALL_FUNCTION_MAPPING = merge_tool_mappings(
        TRANSACTION_FUNCTION_MAPPING, MEMBERSHIP_FUNCTION_MAPPING, BUDGET_FUNCTION_MAPPING,
        ACCOUNT_FUNCTION_MAPPING, CALCULATOR_FUNCTION_MAPPING
    )
except NameError as e:
    logger.critical(f"CRITICAL STARTUP ERROR: {e}")
    raise SystemExit(f"CRITICAL STARTUP ERROR: {e}")

ALL_TOOLS_JSON_STRING = json.dumps(ALL_AVAILABLE_TOOLS, indent=2)

# --- Prompt Mühendisliği ---
SYSTEM_PROMPT = """You are FinBot, an expert financial assistant. Your capabilities include:
1.  Having a natural, helpful conversation.
2.  Using a set of tools to access and manage a user's financial data.
3.  Performing calculations on financial data when asked.

Based on the user's request and the conversation history, decide if you should have a simple conversation or use a tool.
If you need to use a tool, you MUST respond with ONLY a valid JSON object in a ```json ... ``` block, calling the appropriate tool.
If you are having a conversation, respond as a helpful assistant."""

TOOL_PROMPT_TEMPLATE = """{system_prompt}

Here are the tools available to you:
{tools_json_string}

**Conversation History:**
{history_str}
<|user|>
{user_message}
<|end|>
<|assistant|>
"""

def get_summarization_prompt(tool_name: str, function_result: dict) -> str:
    result_str = json.dumps(function_result, cls=DecimalEncoder)
    return f"""You are a helpful financial assistant. Your task is to summarize the result of an executed tool for the user.

**Executed Tool:** `{tool_name}`
**Tool's JSON Result:** `{result_str}`

**CRITICAL INSTRUCTIONS:**
1.  **Be Natural:** Summarize the result in a natural, conversational, and simple sentence.
2.  **DO NOT mention the tool name** (like '{tool_name}') in your final response. The user does not know what a "tool" is.
3.  **Use the Correct Currency:** Look at the `Currency` field in the JSON result if it exists. If the currency is "TRY", use "Turkish Lira" or the "₺" symbol. If it's "USD", use "$". If no currency is provided, do not assume one. For calculations, just state the number.
4.  **Be Factual:** Do not invent details or add information that is not present in the JSON result. Stick strictly to the data.

**RESPONSE EXAMPLES:**
- If tool was `calculate_sum` and result was `{{"total": "1546.3"}}`, your response should be: "The final total comes out to 1546.30."
- If tool was `get_user_accounts` and result was `[{{"name": "Salary Account", "balance": 5000, "currency": "TRY"}}]`, your response could be: "I found your 'Salary Account' with a balance of 5,000 Turkish Lira."
- If the result contains an error, like `{{"error": "API Error", "details": "Account not found"}}`, your response should be: "I couldn't find that account, it seems there was an error."

Now, based on the rules above, summarize the provided tool result for the user.
"""

# --- Yardımcı Fonksiyonlar ---
def _call_ollama(prompt: str, is_json_mode: bool = False, timeout: int = 120) -> str:
    payload = {"model": OLLAMA_MODEL_NAME, "prompt": prompt, "stream": False, "options": {"temperature": 0.1}}
    if is_json_mode:
        payload["format"] = "json"
    
    logger.info(f"Calling Ollama with JSON mode: {is_json_mode}")
    response = requests.post(f"{OLLAMA_API_URL}/api/generate", json=payload, timeout=timeout)
    response.raise_for_status()
    response_text = response.json().get("response", "")
    logger.info(f"Ollama raw response: {response_text}")
    return response_text

# --- API Endpoint'leri ---
@app.get("/metrics", response_class=PlainTextResponse)
async def metrics():
    return generate_latest()

@app.post("/chat", response_model=ChatResponse)
async def chat_endpoint(request: ChatRequest = Body(...)):
    start_time = time.time()
    logger.info(f"Request received: UserId={request.userId}, SessionId={request.clientChatSessionId}, Message='{request.message}'")
    MESSAGES_PROCESSED_TOTAL.inc()
    
    final_reply_text = ""

    try:
        history_str = "\n".join([f"<|{msg.role}|>\n{msg.content}<|end|>" for msg in request.history])
        
        main_prompt = TOOL_PROMPT_TEMPLATE.format(
            system_prompt=SYSTEM_PROMPT,
            tools_json_string=ALL_TOOLS_JSON_STRING,
            history_str=history_str,
            user_message=request.message
        )

        model_response_str = _call_ollama(main_prompt, is_json_mode=True)
        
        tool_call_data = None
        try:
            # Modelin yanıtını her zaman JSON olarak işlemeye çalış
            if model_response_str.strip().startswith('{'):
                # ```json ... ``` bloğunu temizle (varsa)
                if "```json" in model_response_str:
                    model_response_str = model_response_str.split("```json")[1].split("```")[0].strip()
                
                logger.info(f"Attempting to parse model response as JSON: {model_response_str}")
                tool_call_data = json.loads(model_response_str)
            else:
                 # Eğer yanıt JSON değilse, bu bir sohbet mesajıdır.
                 logger.info("Model response is not JSON, treating as chat.")
                 final_reply_text = model_response_str

        except json.JSONDecodeError:
            logger.warning(f"Model response looked like JSON but failed to parse. Treating as chat. Raw: {model_response_str}")
            final_reply_text = model_response_str

        if tool_call_data:
            # DEĞİŞİKLİK: Hem 'tool_to_call' hem de 'action' anahtarlarını kontrol et
            tool_name = tool_call_data.get("tool_to_call") or tool_call_data.get("action")
            
            if tool_name and tool_name in ALL_FUNCTION_MAPPING:
                logger.info(f"Executing tool: '{tool_name}' with args: {tool_call_data.get('parameters', {})}")
                python_function = ALL_FUNCTION_MAPPING[tool_name]
                tool_args = tool_call_data.get("parameters", {})
                
                if "auth_token" in inspect.signature(python_function).parameters:
                    if not request.authToken:
                        raise ValueError("Authentication token is required for this operation but was not provided.")
                    tool_args["auth_token"] = request.authToken
                
                function_result = python_function(**tool_args)
                logger.info(f"Tool '{tool_name}' executed with result: {function_result}")
                
                summarization_prompt = get_summarization_prompt(tool_name, function_result)
                final_reply_text = _call_ollama(summarization_prompt, timeout=60)
            else:
                # Model bir JSON döndürdü ama geçerli bir araç adı içermiyor
                logger.warning(f"Model returned valid JSON but with an unknown tool: '{tool_name}'. Treating as chat.")
                final_reply_text = "I'm not sure how to proceed with that information. Could you clarify what you'd like me to do?"
        
        if not final_reply_text or not final_reply_text.strip():
            logger.warning("Final reply text is empty. Using a fallback message.")
            final_reply_text = "I'm sorry, I'm having trouble formulating a response. Could you try again?"

        current_utc_time = datetime.now(timezone.utc)
        FINBOT_RESPONSE_TIME.observe(time.time() - start_time)
        logger.info(f"Final reply for SessionId={request.clientChatSessionId}: '{final_reply_text.strip()}'")
        
        return ChatResponse(reply=final_reply_text.strip(), responseTime=current_utc_time)

    except Exception as e:
        logger.error(f"An unexpected error occurred in chat endpoint for SessionId={request.clientChatSessionId}: {e}", exc_info=True)
        error_reply = "I'm sorry, I encountered an internal issue and can't respond right now. The technical team has been notified."
        return ChatResponse(reply=error_reply, responseTime=datetime.now(timezone.utc))

# --- Sunucu Başlatma ---
if __name__ == "__main__":
    import uvicorn
    logger.info("Python ChatBot service (Ollama EN - Expert Version with detailed logging) is starting...")
    uvicorn.run(app, host="0.0.0.0", port=8000)