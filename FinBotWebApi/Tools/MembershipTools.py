# -*- coding: windows-1254 -*-

import logging
import os
from typing import List, Dict, Any, Optional
import requests
from dotenv import load_dotenv

load_dotenv()

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

FINTRACK_API_BASE_URL = os.getenv("FINTRACK_API_BASE_URL", "http://localhost:5246")

def _make_api_request(endpoint: str, auth_token: Optional[str], method: str = "GET", params: Optional[Dict] = None, json_data: Optional[Dict] = None) -> Dict[str, Any] | List[Dict[str, Any]]:
    if not auth_token:
        logger.error("Auth Token not provided for API request. Endpoint: %s", endpoint)
        return {"error": "Authentication information is missing."}

    headers = {
        "Authorization": f"Bearer {auth_token}",
        "Accept": "application/json"
    }
    if method.upper() in ["POST", "PUT"] and json_data is not None:
        headers["Content-Type"] = "application/json"

    url = f"{FINTRACK_API_BASE_URL}{endpoint}"
    
    try:
        logger.info(f"Python: Sending request to FinTrack API: {method} {url}, Params: {params}, Data: {json_data}")
        if method.upper() == "GET":
            response = requests.get(url, headers=headers, params=params, timeout=15)
        elif method.upper() == "POST":
            response = requests.post(url, headers=headers, json=json_data, timeout=15)
        elif method.upper() == "PUT":
            response = requests.put(url, headers=headers, json=json_data, timeout=15)
        else:
            logger.error(f"Unsupported HTTP method: {method}")
            return {"error": f"Unsupported HTTP method: {method}"}

        response.raise_for_status() 
        if response.status_code == 204:
             logger.info(f"FinTrack API returned 204 No Content. Endpoint: {url}")
             return {} if method.upper() != "GET" or "history" not in endpoint else []
        
        return response.json()
    except requests.exceptions.HTTPError as http_err:
        logger.error(f"Python: FinTrack API HTTP Error: {http_err} - Response: {http_err.response.text if http_err.response else 'No response'}")
        try:
            error_detail = http_err.response.json() if http_err.response else {"message": "No error details received from server."}
            return {"error": f"API Error: {http_err.response.status_code if http_err.response else 'Unknown'}", "details": error_detail}
        except ValueError:
            return {"error": f"API Error: {http_err.response.status_code if http_err.response else 'Unknown'}", "details": http_err.response.text if http_err.response else 'No response'}
    except requests.exceptions.RequestException as req_err:
        logger.error(f"Python: FinTrack API Request Error: {req_err}")
        return {"error": f"Unable to reach FinTrack API: {req_err}"}
    except Exception as e:
        logger.error(f"Python: General error in FinTrack API request: {e}", exc_info=True)
        return {"error": f"An unknown error occurred: {e}"}

GET_CURRENT_USER_MEMBERSHIP_TOOL = {
    "name": "get_current_user_active_membership",
    "description": "Kullanıcının FinTrack sistemindeki mevcut aktif üyelik planının durumunu, adını ve geçerlilik tarihlerini getirir.",
    "parameters": {
        "type": "OBJECT",
        "properties": {},
        "required": []
    }
}

GET_USER_MEMBERSHIP_HISTORY_TOOL = {
    "name": "get_user_membership_history",
    "description": "Kullanıcının FinTrack sistemindeki tüm geçmiş üyeliklerini (aktif, süresi dolmuş, iptal edilmiş) listeler.",
    "parameters": {
        "type": "OBJECT",
        "properties": {},
        "required": []
    }
}

def get_current_user_active_membership(auth_token: Optional[str]) -> Dict[str, Any]:
    """
    Retrieves the current active membership status of the user from the FinTrack API.
    """
    logger.info(f"Python: get_current_user_active_membership called.")
    return _make_api_request("/api/Membership/current", auth_token)

def get_user_membership_history(auth_token: Optional[str]) -> List[Dict[str, Any]]:
    """
    Retrieves the entire membership history of the user from the FinTrack API.
    """
    logger.info(f"Python: get_user_membership_history called.")
    result = _make_api_request("/api/Membership/history", auth_token)

    return result if isinstance(result, list) else [result] if isinstance(result, dict) and "error" in result else []

MEMBERSHIP_AVAILABLE_TOOLS = [
    GET_CURRENT_USER_MEMBERSHIP_TOOL,
    GET_USER_MEMBERSHIP_HISTORY_TOOL,
]

MEMBERSHIP_FUNCTION_MAPPING = {
    "get_current_user_active_membership": get_current_user_active_membership,
    "get_user_membership_history": get_user_membership_history,
}