import logging
import os
from typing import List, Dict, Any, Optional
import requests
from dotenv import load_dotenv

load_dotenv()

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

FINTRACK_API_BASE_URL = os.getenv("FINTRACK_API_BASE_URL", "http://localhost:5246")

GET_ALL_TRANSACTIONS_TOOL = {
    "name": "get_all_user_transactions",
    "description": "Kullanıcının FinTrack sistemindeki tüm gelir ve gider işlemlerini listeler.",
    "parameters": {
        "type": "OBJECT",
        "properties": {},
        "required": []
    }
}

GET_TRANSACTIONS_BY_CATEGORY_TYPE_TOOL = {
    "name": "get_user_transactions_by_category_type",
    "description": "Kullanıcının belirli bir kategori türündeki (örneğin 'Gelir' veya 'Gider') tüm işlemlerini listeler.",
    "parameters": {
        "type": "OBJECT",
        "properties": {
            "category_type": {
                "type": "STRING",
                "description": "Listelenecek işlemlerin kategori türü (örneğin 'Gelir', 'Gider')."
            }
        },
        "required": ["category_type"]
    }
}

GET_TRANSACTIONS_BY_CATEGORY_NAME_TOOL = {
    "name": "get_user_transactions_by_category_name",
    "description": "Kullanıcının belirli bir kategori adına sahip (örneğin 'Maaş', 'Market Alışverişi') tüm işlemlerini listeler.",
    "parameters": {
        "type": "OBJECT",
        "properties": {
            "category_name": {
                "type": "STRING",
                "description": "Listelenecek işlemlerin kategori adı (örneğin 'Maaş', 'Yemek')."
            }
        },
        "required": ["category_name"]
    }
}

AVAILABLE_TOOLS = [
    GET_ALL_TRANSACTIONS_TOOL,
    GET_TRANSACTIONS_BY_CATEGORY_TYPE_TOOL,
    GET_TRANSACTIONS_BY_CATEGORY_NAME_TOOL,
]

def _make_fin_track_api_request(endpoint: str, auth_token: Optional[str], method: str = "GET", params: Optional[Dict] = None, json_data: Optional[Dict] = None) -> List[Dict[str, Any]]:
    """Helper function to make requests to FinTrack API."""
    if not auth_token:
        logger.error("Auth Token not provided for FinTrack API request.")
        return [{"error": "Authentication information is missing."}]

    headers = {
        "Authorization": f"Bearer {auth_token}",
        "Accept": "application/json"
    }
    url = f"{FINTRACK_API_BASE_URL}{endpoint}"
    
    try:
        logger.info(f"Python: Sending request to FinTrack API: {method} {url}, Params: {params}")
        if method.upper() == "GET":
            response = requests.get(url, headers=headers, params=params, timeout=10)
        elif method.upper() == "POST":
            headers["Content-Type"] = "application/json"
            response = requests.post(url, headers=headers, json=json_data, timeout=10)
        else:
            logger.error(f"Unsupported HTTP method: {method}")
            return [{"error": f"Unsupported HTTP method: {method}"}]

        response.raise_for_status()
        return response.json()
    except requests.exceptions.HTTPError as http_err:
        logger.error(f"Python: FinTrack API HTTP Error: {http_err} - Response: {http_err.response.text}")
        try:
            error_detail = http_err.response.json()
            return [{"error": f"API Error: {http_err.response.status_code}", "details": error_detail}]
        except ValueError: # JSON parse error
            return [{"error": f"API Error: {http_err.response.status_code}", "details": http_err.response.text}]
    except requests.exceptions.RequestException as req_err:
        logger.error(f"Python: FinTrack API Request Error: {req_err}")
        return [{"error": f"Unable to reach FinTrack API: {req_err}"}]
    except Exception as e:
        logger.error(f"Python: General error in FinTrack API request: {e}", exc_info=True)
        return [{"error": f"An unknown error occurred: {e}"}]


def get_all_user_transactions(auth_token: Optional[str]) -> List[Dict[str, Any]]:
    """
    Lists all income and expense transactions of the user in the FinTrack system.
    auth_token: User's JWT.
    """
    logger.info(f"Python: get_all_user_transactions called.")
    return _make_fin_track_api_request("/api/Transactions", auth_token)

def get_user_transactions_by_category_type(category_type: str, auth_token: Optional[str]) -> List[Dict[str, Any]]:
    """
    Lists all transactions of the user for a specific category type.
    category_type: 'Income' or 'Expense'.
    auth_token: User's JWT.
    """
    logger.info(f"Python: get_user_transactions_by_category_type called. CategoryType: {category_type}")
    return _make_fin_track_api_request(f"/api/Transactions/category-type/{category_type}", auth_token)

def get_user_transactions_by_category_name(category_name: str, auth_token: Optional[str]) -> List[Dict[str, Any]]:
    """
    Lists all transactions of the user for a specific category name.
    category_name: For example 'Salary', 'Groceries'.
    auth_token: User's JWT.
    """
    logger.info(f"Python: get_user_transactions_by_category_name called. CategoryName: {category_name}")
    return _make_fin_track_api_request(f"/api/Transactions/category-name/{category_name}", auth_token)


FUNCTION_MAPPING = {
    "get_all_user_transactions": get_all_user_transactions,
    "get_user_transactions_by_category_type": get_user_transactions_by_category_type,
    "get_user_transactions_by_category_name": get_user_transactions_by_category_name,
}