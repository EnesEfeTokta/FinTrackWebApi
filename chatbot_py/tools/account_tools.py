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
        elif method.upper() == "DELETE":
            response = requests.delete(url, headers=headers, timeout=15)
        else:
            logger.error(f"Unsupported HTTP method: {method}")
            return {"error": f"Unsupported HTTP method: {method}"}

        if response.status_code == 204:
             logger.info(f"FinTrack API returned 204 No Content for {method} request. Endpoint: {url}")
             if method.upper() == "DELETE":
                return {"message": "Successfully deleted."}
             return {} 
            
        response.raise_for_status()
        
        return response.json()
    except requests.exceptions.HTTPError as http_err:
        logger.error(f"Python: FinTrack API HTTP Error: {http_err} - Response: {http_err.response.text if http_err.response else 'No response'}")
        try:
            error_detail = http_err.response.json() if http_err.response else {"message": "No error details received from server."}
            return {"error": f"API Error: {http_err.response.status_code if http_err.response else 'Unknown'}", "details": error_detail}
        except ValueError: # JSON parse error
            return {"error": f"API Error: {http_err.response.status_code if http_err.response else 'Unknown'}", "details": http_err.response.text if http_err.response else 'No response'}
    except requests.exceptions.RequestException as req_err:
        logger.error(f"Python: FinTrack API Request Error: {req_err}")
        return {"error": f"Unable to reach FinTrack API: {req_err}"}
    except Exception as e:
        logger.error(f"Python: General error in FinTrack API request: {e}", exc_info=True)
        return {"error": f"An unknown error occurred: {e}"}

GET_USER_ACCOUNTS_TOOL = {
    "name": "get_user_accounts",
    "description": "Kullanıcının FinTrack sistemindeki tüm finansal hesaplarını (örneğin banka hesapları, cüzdanlar) ve mevcut bakiyelerini listeler.",
    "parameters": {
        "type": "OBJECT",
        "properties": {},
        "required": []
    }
}

GET_ACCOUNT_DETAILS_TOOL = {
    "name": "get_account_details",
    "description": "Kullanıcının belirli bir finansal hesabının detaylarını (ID ile) ve bakiyesini getirir.",
    "parameters": {
        "type": "OBJECT",
        "properties": {
            "account_id": {
                "type": "INTEGER",
                "description": "Detayları görüntülenecek hesabın ID'si."
            }
        },
        "required": ["account_id"]
    }
}

CREATE_ACCOUNT_TOOL = {
    "name": "create_account",
    "description": "Kullanıcı için yeni bir finansal hesap (örneğin banka hesabı, nakit cüzdanı) oluşturur.",
    "parameters": {
        "type": "OBJECT",
        "properties": {
            "name": {"type": "STRING", "description": "Oluşturulacak hesabın adı (örneğin 'Maaş Hesabım', 'Nakit Cüzdanı')."},
            "type": {"type": "STRING", "description": "Hesabın türü (örneğin 'Banka', 'Nakit', 'Kredi Kartı', 'Yatırım'). API'nin kabul ettiği türleri kullanın."},
            "balance": {"type": "NUMBER", "description": "Hesabın başlangıç bakiyesi."}
        },
        "required": ["name", "type", "balance"]
    }
}

def get_user_accounts(auth_token: Optional[str]) -> List[Dict[str, Any]]:
    """Fetches all financial accounts of the user from the FinTrack API."""
    logger.info(f"Python: get_user_accounts called.")
    result = _make_api_request("/api/Account", auth_token)
    return result if isinstance(result, list) else [result] if isinstance(result, dict) and "error" in result else []

def get_account_details(account_id: int, auth_token: Optional[str]) -> Dict[str, Any]:
    """Fetches the details of a specific financial account from the FinTrack API."""
    logger.info(f"Python: get_account_details called. AccountID: {account_id}")
    return _make_api_request(f"/api/Account/{account_id}", auth_token)

def create_account(name: str, type: str, balance: float, auth_token: Optional[str]) -> Dict[str, Any]:
    """Creates a new financial account."""
    logger.info(f"Python: create_account called. Name: {name}, Type: {type}, Balance: {balance}")
    payload = {
        "name": name,
        "type": type,
        "balance": balance
    }
    return _make_api_request("/api/Account", auth_token, method="POST", json_data=payload)


ACCOUNT_AVAILABLE_TOOLS = [
    GET_USER_ACCOUNTS_TOOL,
    GET_ACCOUNT_DETAILS_TOOL,
    CREATE_ACCOUNT_TOOL,
]

ACCOUNT_FUNCTION_MAPPING = {
    "get_user_accounts": get_user_accounts,
    "get_account_details": get_account_details,
    "create_account": create_account,
}