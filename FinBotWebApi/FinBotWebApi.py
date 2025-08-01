import os
import logging
import json
import inspect
from typing import List, Dict, Any, Optional

import requests
from fastapi import FastAPI, HTTPException, Body, Request, Response
from fastapi.responses import PlainTextResponse
from pydantic import BaseModel
from dotenv import load_dotenv
from prometheus_client import generate_latest, Counter, Histogram
import time
from datetime import datetime, timezone

from Tools.FinanceTools import AVAILABLE_TOOLS as FINANCE_TOOLS, FUNCTION_MAPPING as FINANCE_FUNCTION_MAPPING
from Tools.MembershipTools import MEMBERSHIP_AVAILABLE_TOOLS, MEMBERSHIP_FUNCTION_MAPPING
from Tools.BudgetTools import BUDGET_AVAILABLE_TOOLS, BUDGET_FUNCTION_MAPPING
from Tools.AccountTools import ACCOUNT_AVAILABLE_TOOLS, ACCOUNT_FUNCTION_MAPPING

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

load_dotenv()

OLLAMA_API_URL = os.getenv("OLLAMA_API_URL", "http://localhost:11434") 
OLLAMA_MODEL_NAME = "phi3:mini"

app = FastAPI(title="FinTrack ChatBot Service (Python with Ollama)")

# HTTP isteklerinin toplam sayısını sayar
REQUEST_COUNT = Counter(
    'finbot_http_requests_total',
    'Total HTTP requests to FinBot',
    ['method', 'endpoint', 'status_code']
)

# HTTP isteklerinin süresini ölçer
REQUEST_LATENCY = Histogram(
    'finbot_http_request_duration_seconds',
    'HTTP request latency in seconds',
    ['endpoint']
)

# İşlenen mesajların toplam sayısını sayar
MESSAGES_PROCESSED_TOTAL = Counter(
    'finbot_messages_processed_total',
    'Total number of messages processed by FinBot'
)

# FinBot yanıt süresi
FINBOT_RESPONSE_TIME = Histogram(
    'finbot_response_duration_seconds',
    'Duration of FinBot processing a request in seconds'
)

# Her gelen isteği izlemek icın
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

# Prometheus metrik endpoint'i
@app.get("/metrics", response_class=PlainTextResponse)
async def metrics():
    """
    Prometheus metriklerini döndürür.
    """
    return generate_latest()

class ChatRequest(BaseModel):
    userId: str
    clientChatSessionId: str
    message: str
    authToken: Optional[str] = None

class ChatResponse(BaseModel):
    reply: str
    responseTime: datetime 

ALL_AVAILABLE_TOOLS = FINANCE_TOOLS + MEMBERSHIP_AVAILABLE_TOOLS + BUDGET_AVAILABLE_TOOLS + ACCOUNT_AVAILABLE_TOOLS
ALL_FUNCTION_MAPPING = {**FINANCE_FUNCTION_MAPPING, **MEMBERSHIP_FUNCTION_MAPPING, **BUDGET_FUNCTION_MAPPING, **ACCOUNT_FUNCTION_MAPPING}
ALL_TOOLS_JSON_STRING = json.dumps(ALL_AVAILABLE_TOOLS, indent=2)

def get_intent_detection_prompt(user_message: str) -> str:
    """
    A much more robust prompt for intent classification, using few-shot examples.
    This helps the model learn the pattern correctly.
    """
    return f"""You are a text classifier. Your only job is to classify the user's intent as either "TOOL" or "CHAT".
Do not explain your reasoning. Do not use any other words.

Here are some examples:
User: "Hello there"
Response: CHAT

User: "how are you?"
Response: CHAT

User: "thanks"
Response: CHAT

User: "list all my accounts"
Response: TOOL

User: "create a new budget for groceries for 500 dollars"
Response: TOOL

User: "what was my last transaction?"
Response: TOOL

---
Now, classify the following user request.

User: "{user_message}"
Response:"""

def get_tool_calling_prompt_english(tools_json_string: str, user_message: str) -> str:
    return f"""<|system|>
Your only task is to analyze the user's request and generate a JSON command to call the appropriate tool from the provided list. Do nothing else.

User Request: "{user_message}"

Available Tools:
{tools_json_string}

Your output MUST ONLY be a JSON object in the following format:
```json
{{
  "tool_to_call": "<tool_name>",
  "parameters": {{
    "<param_name>": "<value>"
  }}
}}
```<|end|>
<|user|>
{user_message}<|end|>
<|assistant|>
"""

def get_simple_chat_prompt_english(user_message: str) -> str:
    return f"""<|system|>
You are FinBot, a friendly and helpful financial assistant. You are having a simple conversation with a user. Keep your answers short, polite, and helpful.<|end|>
<|user|>
{user_message}<|end|>
<|assistant|>
"""

def get_summarization_prompt_english(tool_name: str, function_result: dict) -> str:
    return f"A tool named '{tool_name}' was executed and returned this JSON data: {json.dumps(function_result)}. Summarize this result for the user in a friendly, simple sentence."

@app.post("/chat", response_model=ChatResponse)
async def chat_endpoint(request: ChatRequest = Body(...)):
    logger.info(f"Python (Ollama-EN-v2): Request received: UserId={request.userId}, Message='{request.message}'")
    final_reply_text = "I'm sorry, I encountered a problem while processing your request."

    try:
        intent_prompt = get_intent_detection_prompt(request.message)
        intent_payload = {"model": OLLAMA_MODEL_NAME, "prompt": intent_prompt, "stream": False}
        
        logger.info("Python (Ollama-EN-v2): Detecting intent...")
        intent_response = requests.post(f"{OLLAMA_API_URL}/api/generate", json=intent_payload, timeout=30)
        intent_response.raise_for_status()
        
        intent = intent_response.json().get("response", "").strip().upper()
        logger.info(f"Python (Ollama-EN-v2): Detected intent: '{intent}'")

        if "TOOL" in intent:
            logger.info("Python (Ollama-EN-v2): Following TOOL path.")
            tool_prompt = get_tool_calling_prompt_english(ALL_TOOLS_JSON_STRING, request.message)
            tool_payload = {"model": OLLAMA_MODEL_NAME, "prompt": tool_prompt, "format": "json", "stream": False}

            tool_response = requests.post(f"{OLLAMA_API_URL}/api/generate", json=tool_payload, timeout=120)
            tool_response.raise_for_status()
            
            model_response_str = tool_response.json().get("response", "")
            
            try:
                tool_call_data = json.loads(model_response_str)
                tool_name = tool_call_data.get("tool_to_call")
                tool_args = tool_call_data.get("parameters", {})
                
                if tool_name in ALL_FUNCTION_MAPPING:
                    python_function = ALL_FUNCTION_MAPPING[tool_name]
                    if "auth_token" in inspect.signature(python_function).parameters:
                        if not request.authToken: raise ValueError("Auth token is required for this operation.")
                        tool_args["auth_token"] = request.authToken
                    
                    function_result = python_function(**tool_args)
                    
                    summarization_prompt = get_summarization_prompt_english(tool_name, function_result)
                    summary_payload = {"model": OLLAMA_MODEL_NAME, "prompt": summarization_prompt, "stream": False}
                    summary_response = requests.post(f"{OLLAMA_API_URL}/api/generate", json=summary_payload, timeout=60)
                    summary_response.raise_for_status()
                    final_reply_text = summary_response.json().get("response", "Your request has been processed.")
                else:
                    final_reply_text = "I could not find a suitable tool for your request."

            except (json.JSONDecodeError, KeyError):
                logger.error(f"Failed to parse tool call JSON: {model_response_str}")
                final_reply_text = "There was an issue trying to use a tool."

        else:
            logger.info("Python (Ollama-EN-v2): Following CHAT path.")
            chat_prompt = get_simple_chat_prompt_english(request.message)
            chat_payload = {"model": OLLAMA_MODEL_NAME, "prompt": chat_prompt, "stream": False}
            
            chat_response = requests.post(f"{OLLAMA_API_URL}/api/generate", json=chat_payload, timeout=60)
            chat_response.raise_for_status()
            final_reply_text = chat_response.json().get("response", "Hello! How can I help you today?")

        current_utc_time = datetime.now(timezone.utc)
        
        return ChatResponse(reply=final_reply_text, responseTime=current_utc_time)

    except requests.exceptions.RequestException as req_err:
        logger.error(f"Python (Ollama-EN-v2): Could not connect to Ollama API: {req_err}")
        raise HTTPException(status_code=503, detail="The AI service is currently unavailable.")
    except Exception as e:
        logger.error(f"Python (Ollama-EN-v2): General error in chat endpoint: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"An error occurred in the ChatBot service: {str(e)}")

if __name__ == "__main__":
    import uvicorn
    logger.info("Python ChatBot service (Ollama EN version) is starting...")
    uvicorn.run(app, host="0.0.0.0", port=8000)