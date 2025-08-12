import os
import logging
import json
import inspect
import re
from typing import List, Dict, Any, Optional

import requests
from fastapi import FastAPI, HTTPException, Body, Request
from fastapi.responses import PlainTextResponse
from pydantic import BaseModel, Field
from dotenv import load_dotenv
from prometheus_client import generate_latest, Counter, Histogram
import time
from datetime import datetime, timezone

from Tools.TransactionTools import TRANSACTION_AVAILABLE_TOOLS, TRANSACTION_FUNCTION_MAPPING
from Tools.MembershipTools import MEMBERSHIP_AVAILABLE_TOOLS, MEMBERSHIP_FUNCTION_MAPPING
from Tools.BudgetTools import BUDGET_AVAILABLE_TOOLS, BUDGET_FUNCTION_MAPPING
from Tools.AccountTools import ACCOUNT_AVAILABLE_TOOLS, ACCOUNT_FUNCTION_MAPPING
from Tools.CalculatorTools import CALCULATOR_AVAILABLE_TOOLS, CALCULATOR_FUNCTION_MAPPING
from Tools.FaqTools import FAQ_AVAILABLE_TOOLS, FAQ_FUNCTION_MAPPING

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - [%(filename)s:%(lineno)d] - %(message)s')
logger = logging.getLogger(__name__)
load_dotenv()

OLLAMA_API_URL = os.getenv("OLLAMA_API_URL", "http://localhost:11434")
OLLAMA_MODEL_NAME = "mistral:instruct"
app = FastAPI(title="FinTrack ChatBot Service - User-Centric Edition")

REQUEST_COUNT = Counter('finbot_http_requests_total', 'Total HTTP requests', ['method', 'endpoint', 'status_code'])
REQUEST_LATENCY = Histogram('finbot_http_request_duration_seconds', 'HTTP request latency', ['endpoint'])
MESSAGES_PROCESSED_TOTAL = Counter('finbot_messages_processed_total', 'Total messages processed')
FINBOT_RESPONSE_TIME = Histogram('finbot_response_duration_seconds', 'FinBot response duration')

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
    BUDGET_AVAILABLE_TOOLS + ACCOUNT_AVAILABLE_TOOLS + CALCULATOR_AVAILABLE_TOOLS +
    FAQ_AVAILABLE_TOOLS
)
try:
    ALL_FUNCTION_MAPPING = merge_tool_mappings(
        TRANSACTION_FUNCTION_MAPPING, MEMBERSHIP_FUNCTION_MAPPING, BUDGET_FUNCTION_MAPPING,
        ACCOUNT_FUNCTION_MAPPING, CALCULATOR_FUNCTION_MAPPING,
        FAQ_FUNCTION_MAPPING
    )
except NameError as e:
    logger.error(e)
    raise SystemExit(f"CRITICAL STARTUP ERROR: {e}")

ALL_TOOLS_JSON_STRING = json.dumps(ALL_AVAILABLE_TOOLS, indent=2)

SYSTEM_PROMPT = """You are FinBot, an expert, proactive, and transparent financial assistant. Your primary goal is to help the user while making them feel in control and informed.

**Your Core Persona:**
- **Friendly & Empathetic:** Always be polite and acknowledge the user's goal.
- **Transparent:** Before you perform an action (use a tool), explain what you are about to do and why.
- **Proactive:** If a user's request is ambiguous, ask clarifying questions instead of guessing.

**Your Workflow (Reason, Confirm, Act):**
1.  **Reason:** Analyze the user's request and conversation history to form a plan. This might involve one or more tool calls.
2.  **Confirm & Act (The MOST IMPORTANT step):**
    -   If a tool is needed, **DO NOT** output the tool's JSON immediately. Instead, first, respond with a clear, conversational message explaining your plan. For example: "To calculate your remaining balance, I first need to fetch your latest transactions. Is that okay?"
    -   If the user's request requires a tool call to proceed, you **MUST** respond with the tool's JSON object **ONLY** in a ```json ... ``` block. This will be triggered by a specific directive in the prompt.
    -   If no tool is needed, just have a normal, helpful conversation.

**You will be given a specific TASK in the prompt. Follow it precisely.**"""

def get_generation_prompt(tools_json_string: str, user_message: str, history: List[ChatMessage]) -> str:
    history_str = "\n".join([f"<|{msg.role}|>\n{msg.content}<|end|>" for msg in history])

    return f"""{SYSTEM_PROMPT}

**Available Tools:**
{tools_json_string}

**Conversation History:**
{history_str}
<|user|>
{user_message}
<|end|>
<|assistant|>
"""

def get_forced_tool_call_prompt(original_prompt_context: str) -> str:
    return f"""{original_prompt_context}
**TASK:** The user has confirmed. Your only task now is to generate the JSON for the next logical tool call based on the conversation. Respond with **ONLY** the JSON object in a ```json ... ``` block. Do not add any other text."""

def get_summarization_prompt(tool_name: str, function_result: dict, user_request: str) -> str:
    result_str = json.dumps(function_result)
    return f"""The user's request was: "{user_request}".
A tool named '{tool_name}' was just executed and returned this data: {result_str}.

Your task is to present this result to the user in a helpful, clear, and conversational way.
- **If successful:** Explain what the data means. Don't just list it. For example, instead of "Here is a list...", say "I found three accounts for you: your 'Main Checking' has $500, and...".
- **If data is empty:** Reassure the user. For example: "It looks like you don't have any budgets set up yet. Would you like to create one?"
- **If error:** Apologize and explain the error simply. Example: "I'm sorry, I couldn't find an account with that ID. Could you please double-check the number?"

Now, generate a user-friendly response."""

def _call_ollama(prompt: str, timeout: int = 120) -> str:
    payload = {"model": OLLAMA_MODEL_NAME, "prompt": prompt, "stream": False, "options": {"temperature": 0.2, "stop": ["<|end|>"]}}
    logger.info("Calling Ollama...")
    response = requests.post(f"{OLLAMA_API_URL}/api/generate", json=payload, timeout=timeout)
    response.raise_for_status()
    response_text = response.json().get("response", "").strip()
    logger.info(f"Ollama raw response: {response_text}")
    return response_text

def _extract_json_from_response(text: str) -> Optional[Dict[str, Any]]:
    match = re.search(r"```json\s*(\{.*?\})\s*```", text, re.DOTALL)
    if match:
        json_str = match.group(1)
        try:
            return json.loads(json_str)
        except json.JSONDecodeError:
            logger.error(f"Failed to decode extracted JSON: {json_str}")
            return None
    return None

@app.get("/metrics", response_class=PlainTextResponse)
async def metrics():
    return generate_latest()

@app.post("/chat", response_model=ChatResponse)
async def chat_endpoint(request: ChatRequest = Body(...)):
    start_time = time.time()
    logger.info(f"Request received: UserId={request.userId}, Message='{request.message}'")
    MESSAGES_PROCESSED_TOTAL.inc()
    
    final_reply_text = "I'm sorry, I encountered an issue and can't respond right now."

    try:
        is_confirmation = request.message.lower().strip() in ["yes", "yep", "ok", "okay", "proceed", "sure", "do it"]

        generation_prompt = get_generation_prompt(ALL_TOOLS_JSON_STRING, request.message, request.history)
        
        if is_confirmation and request.history:
            forced_tool_prompt = get_forced_tool_call_prompt(generation_prompt)
            model_response_str = _call_ollama(forced_tool_prompt)
        else:
            model_response_str = _call_ollama(generation_prompt)
        
        tool_call_data = _extract_json_from_response(model_response_str)

        if tool_call_data:
            tool_name = tool_call_data.get("name")
            tool_args = tool_call_data.get("arguments", {})
            
            if tool_name and tool_name in ALL_FUNCTION_MAPPING:
                logger.info(f"Executing tool: '{tool_name}' with args: {tool_args}")
                python_function = ALL_FUNCTION_MAPPING[tool_name]
                
                if "auth_token" in inspect.signature(python_function).parameters:
                    if not request.authToken:
                        raise ValueError("Authentication token is required but was not provided.")
                    tool_args["auth_token"] = request.authToken
                
                function_result = python_function(**tool_args)
                
                summarization_prompt = get_summarization_prompt(tool_name, function_result, request.message)
                final_reply_text = _call_ollama(summarization_prompt, timeout=60)
            else:
                logger.warning(f"Model returned JSON for an unknown tool: '{tool_name}'.")
                final_reply_text = "I seem to have called a tool that doesn't exist. My apologies. Could you rephrase?"
        else:
            final_reply_text = model_response_str

        if not final_reply_text or not final_reply_text.strip():
            logger.warning("Ollama returned an empty response. Using a fallback message.")
            final_reply_text = "I'm sorry, I'm having trouble formulating a response. Could you try again?"

        current_utc_time = datetime.now(timezone.utc)
        FINBOT_RESPONSE_TIME.observe(time.time() - start_time)
        logger.info(f"Final reply for SessionId={request.clientChatSessionId}: '{final_reply_text.strip()}'")
        
        return ChatResponse(reply=final_reply_text.strip(), responseTime=current_utc_time)

    except requests.exceptions.RequestException as req_err:
        logger.error(f"Could not connect to Ollama API: {req_err}")
        raise HTTPException(status_code=503, detail="The AI service is currently unavailable.")
    except Exception as e:
        logger.error(f"General error in chat endpoint: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail="An unexpected error occurred in the ChatBot service.")

if __name__ == "__main__":
    import uvicorn
    logger.info("Python ChatBot service (Ollama EN - User-Centric Final Version) is starting...")
    uvicorn.run(app, host="0.0.0.0", port=8000)