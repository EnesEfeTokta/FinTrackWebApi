# -*- coding: windows-1254 -*-

import decimal
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
        logger.error("API iste�i i�in Auth Token sa�lanmad�. Endpoint: %s", endpoint)
        return {"error": "Kimlik do�rulama bilgisi eksik."}

    headers = {
        "Authorization": f"Bearer {auth_token}",
        "Accept": "application/json"
    }
    if method.upper() in ["POST", "PUT"] and json_data is not None:
        headers["Content-Type"] = "application/json"

    url = f"{FINTRACK_API_BASE_URL}{endpoint}"
    
    try:
        logger.info(f"Python: FinTrack API'sine istek g�nderiliyor: {method} {url}, Params: {params}, Data: {json_data}")
        if method.upper() == "GET":
            response = requests.get(url, headers=headers, params=params, timeout=15)
        elif method.upper() == "POST":
            response = requests.post(url, headers=headers, json=json_data, timeout=15)
        elif method.upper() == "PUT":
            response = requests.put(url, headers=headers, json=json_data, timeout=15)
        elif method.upper() == "DELETE":
            response = requests.delete(url, headers=headers, timeout=15)
        else:
            logger.error(f"Desteklenmeyen HTTP metodu: {method}")
            return {"error": f"Desteklenmeyen HTTP metodu: {method}"}

        if response.status_code == 204 and method.upper() == "DELETE":
            logger.info(f"FinTrack API'den {method} iste�i i�in 204 No Content yan�t� al�nd�. Endpoint: {url}")
            return {"message": "��lem ba�ar�yla silindi."}
            
        response.raise_for_status()
        
        if response.status_code == 204:
             logger.info(f"FinTrack API'den 204 No Content yan�t� al�nd�. Endpoint: {url}")
             return {} if method.upper() != "GET" or "history" not in endpoint else []
        
        return response.json()
    except requests.exceptions.HTTPError as http_err:
        logger.error(f"Python: FinTrack API HTTP Hatas�: {http_err} - Yan�t: {http_err.response.text if http_err.response else 'Yan�t yok'}")
        try:
            error_detail = http_err.response.json() if http_err.response else {"message": "Sunucudan hata detay� al�namad�."}
            return {"error": f"API Hatas�: {http_err.response.status_code if http_err.response else 'Bilinmiyor'}", "details": error_detail}
        except ValueError:
            return {"error": f"API Hatas�: {http_err.response.status_code if http_err.response else 'Bilinmiyor'}", "details": http_err.response.text if http_err.response else 'Yan�t yok'}
    except requests.exceptions.RequestException as req_err:
        logger.error(f"Python: FinTrack API �stek Hatas�: {req_err}")
        return {"error": f"FinTrack API'sine ula��lamad�: {req_err}"}
    except Exception as e:
        logger.error(f"Python: FinTrack API iste�inde genel hata: {e}", exc_info=True)
        return {"error": f"Bilinmeyen bir hata olu�tu: {e}"}

GET_USER_BUDGETS_TOOL = {
    "name": "get_user_budgets",
    "description": "Kullan�c�n�n FinTrack sistemindeki t�m b�t�elerini listeler.",
    "parameters": {
        "type": "OBJECT",
        "properties": {},
        "required": []
    }
}

GET_BUDGET_DETAILS_TOOL = {
    "name": "get_budget_details",
    "description": "Kullan�c�n�n belirli bir b�t�esinin detaylar�n� (ID ile) getirir.",
    "parameters": {
        "type": "OBJECT",
        "properties": {
            "budget_id": {
                "type": "INTEGER",
                "description": "Detaylar� g�r�nt�lenecek b�t�enin ID'si."
            }
        },
        "required": ["budget_id"]
    }
}

CREATE_BUDGET_TOOL = {
    "name": "create_budget",
    "description": "Kullan�c� i�in yeni bir b�t�e olu�turur.",
    "parameters": {
        "type": "OBJECT",
        "properties": {
            "name": {"type": "STRING", "description": "B�t�enin ad� (�rne�in 'Ayl�k Harcamalar', 'Tatil Fonu')."},
            "description": {"type": "STRING", "description": "B�t�e i�in k�sa bir a��klama (iste�e ba�l�)."},
            "category": {"type": "STRING", "description": "B�t�enin kategorisi (�rne�in 'G�da', 'Ula��m', 'E�lence')."},
            "start_date": {"type": "STRING", "description": "B�t�enin ba�lang�� tarihi (YYYY-AA-GG format�nda olmal� ama kullan�c� di�er formatlarda girer ise onu da YYYY-AA-GG format�na �evir.)."},
            "allocatedAmount": {"type": "NUMBER", "description": "B�t�eye ayr�lan toplam miktar (iste�e ba�l�, varsay�lan: 0)."},
            "currency" : {"type": "STRING", "description": "B�t�enin para birimi (Sadece alabilece�i de�erler 'TRY', 'USD', 'EUR', 'GBP', 'JPY', 'AUD', 'CAD', 'CHF')."},
            "end_date": {"type": "STRING", "description": "B�t�enin biti� tarihi (YYYY-AA-GG format�nda olmal� ama kullan�c� di�er formatlarda girer ise onu da YYYY-AA-GG format�na �evir.)."},
            "is_active": {"type": "BOOLEAN", "description": "B�t�enin aktif olup olmad��� (varsay�lan: true)."}
        },
        "required": ["name", "category", "allocatedAmount", "currency", "start_date", "end_date"]
    }
}

def get_user_budgets(auth_token: Optional[str]) -> List[Dict[str, Any]]:
    """Kullan�c�n�n t�m b�t�elerini FinTrack API'sinden al�r."""
    logger.info(f"Python: get_user_budgets �a�r�ld�.")
    result = _make_api_request("/Budgets", auth_token)
    return result if isinstance(result, list) else [result] if isinstance(result, dict) and "error" in result else []

def get_budget_details(budget_id: int, auth_token: Optional[str]) -> Dict[str, Any]:
    """Fetches the details of a specific budget from the FinTrack API."""
    logger.info(f"Python: get_budget_details called. BudgetID: {budget_id}")
    return _make_api_request(f"/Budgets/{budget_id}", auth_token)

def create_budget(
    name: str, 
    start_date: str, 
    end_date: str, 
    allocatedAmount: decimal,
    currency: str,
    category: str,
    description: Optional[str] = None, 
    is_active: bool = True, 
    auth_token: Optional[str] = None) -> Dict[str, Any]:
    """Creates a new budget."""
    logger.info(f"Python: create_budget called. Name: {name}, Start: {start_date}, End: {end_date}")
    payload = {
        "name": name,
        "description": description,
        "category": category,
        "startDate": start_date,
        "endDate": end_date,
        "allocatedAmount": allocatedAmount,
        "currency": currency.upper(),
        "isActive": is_active
    }
    return _make_api_request("/Budgets", auth_token, method="POST", json_data=payload)

BUDGET_AVAILABLE_TOOLS = [
    GET_USER_BUDGETS_TOOL,
    GET_BUDGET_DETAILS_TOOL,
    CREATE_BUDGET_TOOL,
]

BUDGET_FUNCTION_MAPPING = {
    "get_user_budgets": get_user_budgets,
    "get_budget_details": get_budget_details,
    "create_budget": create_budget,
}