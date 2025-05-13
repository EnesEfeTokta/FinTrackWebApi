# chatbot_py/tools/membership_tools.py
import logging
import os
from typing import List, Dict, Any, Optional
import requests
from dotenv import load_dotenv

load_dotenv()

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

FINTRACK_API_BASE_URL = os.getenv("FINTRACK_API_BASE_URL", "http://localhost:5246") # .env'den oku (portu kontrol edin)

# --- Yardımcı Fonksiyon (FinTrack API'sine istek atmak için) ---
def _make_api_request(endpoint: str, auth_token: Optional[str], method: str = "GET", params: Optional[Dict] = None, json_data: Optional[Dict] = None) -> Dict[str, Any] | List[Dict[str, Any]]:
    if not auth_token:
        logger.error("API isteği için Auth Token sağlanmadı. Endpoint: %s", endpoint)
        return {"error": "Kimlik doğrulama bilgisi eksik."}

    headers = {
        "Authorization": f"Bearer {auth_token}",
        "Accept": "application/json"
    }
    if method.upper() in ["POST", "PUT"] and json_data is not None:
        headers["Content-Type"] = "application/json"

    url = f"{FINTRACK_API_BASE_URL}{endpoint}"
    
    try:
        logger.info(f"Python: FinTrack API'sine istek gönderiliyor: {method} {url}, Params: {params}, Data: {json_data}")
        if method.upper() == "GET":
            response = requests.get(url, headers=headers, params=params, timeout=15)
        elif method.upper() == "POST":
            response = requests.post(url, headers=headers, json=json_data, timeout=15)
        elif method.upper() == "PUT":
            response = requests.put(url, headers=headers, json=json_data, timeout=15)
        # Diğer metotlar (DELETE) eklenebilir
        else:
            logger.error(f"Desteklenmeyen HTTP metodu: {method}")
            return {"error": f"Desteklenmeyen HTTP metodu: {method}"}

        response.raise_for_status() 
        # Yanıtın boş olup olmadığını kontrol et, boşsa boş bir dict veya list döndür
        if response.status_code == 204: # No Content
             logger.info(f"FinTrack API'den 204 No Content yanıtı alındı. Endpoint: {url}")
             return {} if method.upper() != "GET" or "history" not in endpoint else [] # GET history için boş liste
        
        return response.json()
    except requests.exceptions.HTTPError as http_err:
        logger.error(f"Python: FinTrack API HTTP Hatası: {http_err} - Yanıt: {http_err.response.text if http_err.response else 'Yanıt yok'}")
        try:
            error_detail = http_err.response.json() if http_err.response else {"message": "Sunucudan hata detayı alınamadı."}
            return {"error": f"API Hatası: {http_err.response.status_code if http_err.response else 'Bilinmiyor'}", "details": error_detail}
        except ValueError:
            return {"error": f"API Hatası: {http_err.response.status_code if http_err.response else 'Bilinmiyor'}", "details": http_err.response.text if http_err.response else 'Yanıt yok'}
    except requests.exceptions.RequestException as req_err:
        logger.error(f"Python: FinTrack API İstek Hatası: {req_err}")
        return {"error": f"FinTrack API'sine ulaşılamadı: {req_err}"}
    except Exception as e:
        logger.error(f"Python: FinTrack API isteğinde genel hata: {e}", exc_info=True)
        return {"error": f"Bilinmeyen bir hata oluştu: {e}"}

# --- Fonksiyon Şemaları (Tool Tanımları) ---
GET_CURRENT_USER_MEMBERSHIP_TOOL = {
    "name": "get_current_user_active_membership",
    "description": "Kullanıcının FinTrack sistemindeki mevcut aktif üyelik planının durumunu, adını ve geçerlilik tarihlerini getirir.",
    "parameters": {
        "type": "OBJECT",
        "properties": {}, # Parametre yok, auth_token ile kullanıcı bilgisi alınacak
        "required": []
    }
}

GET_USER_MEMBERSHIP_HISTORY_TOOL = {
    "name": "get_user_membership_history",
    "description": "Kullanıcının FinTrack sistemindeki tüm geçmiş üyeliklerini (aktif, süresi dolmuş, iptal edilmiş) listeler.",
    "parameters": {
        "type": "OBJECT",
        "properties": {}, # Parametre yok
        "required": []
    }
}

# --- Python Fonksiyonları ---
def get_current_user_active_membership(auth_token: Optional[str]) -> Dict[str, Any]:
    """
    Kullanıcının mevcut aktif üyelik durumunu FinTrack API'sinden alır.
    """
    logger.info(f"Python: get_current_user_active_membership çağrıldı.")
    return _make_api_request("/api/Membership/current", auth_token)

def get_user_membership_history(auth_token: Optional[str]) -> List[Dict[str, Any]]:
    """
    Kullanıcının tüm üyelik geçmişini FinTrack API'sinden alır.
    """
    logger.info(f"Python: get_user_membership_history çağrıldı.")
    result = _make_api_request("/api/Membership/history", auth_token)
    # _make_api_request dict veya list dönebilir. Emin olmak için tip kontrolü.
    return result if isinstance(result, list) else [result] if isinstance(result, dict) and "error" in result else []


# --- Araç ve Fonksiyon Eşleştirme Listeleri (main.py'de birleştirilecek) ---
MEMBERSHIP_AVAILABLE_TOOLS = [
    GET_CURRENT_USER_MEMBERSHIP_TOOL,
    GET_USER_MEMBERSHIP_HISTORY_TOOL,
]

MEMBERSHIP_FUNCTION_MAPPING = {
    "get_current_user_active_membership": get_current_user_active_membership,
    "get_user_membership_history": get_user_membership_history,
}