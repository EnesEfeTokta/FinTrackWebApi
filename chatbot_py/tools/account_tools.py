# chatbot_py/tools/account_tools.py
import logging
import os
from typing import List, Dict, Any, Optional
import requests
from dotenv import load_dotenv

load_dotenv()

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

FINTRACK_API_BASE_URL = os.getenv("FINTRACK_API_BASE_URL", "http://localhost:5246")

# --- Yardımcı Fonksiyon (FinTrack API'sine istek atmak için - diğer tool dosyalarından alınabilir) ---
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
        elif method.upper() == "DELETE":
            response = requests.delete(url, headers=headers, timeout=15)
        else:
            logger.error(f"Desteklenmeyen HTTP metodu: {method}")
            return {"error": f"Desteklenmeyen HTTP metodu: {method}"}

        if response.status_code == 204: # No Content (genellikle DELETE ve bazen PUT için)
             logger.info(f"FinTrack API'den {method} isteği için 204 No Content yanıtı alındı. Endpoint: {url}")
             if method.upper() == "DELETE":
                return {"message": "İşlem başarıyla silindi."}
             return {} # Diğer 204 durumları için boş dict
            
        response.raise_for_status() # Diğer 4xx veya 5xx hatalarında exception fırlatır
        
        return response.json()
    except requests.exceptions.HTTPError as http_err:
        logger.error(f"Python: FinTrack API HTTP Hatası: {http_err} - Yanıt: {http_err.response.text if http_err.response else 'Yanıt yok'}")
        try:
            error_detail = http_err.response.json() if http_err.response else {"message": "Sunucudan hata detayı alınamadı."}
            return {"error": f"API Hatası: {http_err.response.status_code if http_err.response else 'Bilinmiyor'}", "details": error_detail}
        except ValueError: # JSON parse hatası
            return {"error": f"API Hatası: {http_err.response.status_code if http_err.response else 'Bilinmiyor'}", "details": http_err.response.text if http_err.response else 'Yanıt yok'}
    except requests.exceptions.RequestException as req_err:
        logger.error(f"Python: FinTrack API İstek Hatası: {req_err}")
        return {"error": f"FinTrack API'sine ulaşılamadı: {req_err}"}
    except Exception as e:
        logger.error(f"Python: FinTrack API isteğinde genel hata: {e}", exc_info=True)
        return {"error": f"Bilinmeyen bir hata oluştu: {e}"}

# --- Fonksiyon Şemaları (Tool Tanımları) ---
GET_USER_ACCOUNTS_TOOL = {
    "name": "get_user_accounts",
    "description": "Kullanıcının FinTrack sistemindeki tüm finansal hesaplarını (örneğin banka hesapları, cüzdanlar) ve mevcut bakiyelerini listeler.",
    "parameters": {
        "type": "OBJECT",
        "properties": {}, # Parametre yok, auth_token ile kullanıcı bilgisi alınacak
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

# --- Python Fonksiyonları ---
def get_user_accounts(auth_token: Optional[str]) -> List[Dict[str, Any]]:
    """Kullanıcının tüm finansal hesaplarını FinTrack API'sinden alır."""
    logger.info(f"Python: get_user_accounts çağrıldı.")
    result = _make_api_request("/api/Account", auth_token)
    return result if isinstance(result, list) else [result] if isinstance(result, dict) and "error" in result else []

def get_account_details(account_id: int, auth_token: Optional[str]) -> Dict[str, Any]:
    """Belirli bir finansal hesabın detaylarını FinTrack API'sinden alır."""
    logger.info(f"Python: get_account_details çağrıldı. AccountID: {account_id}")
    return _make_api_request(f"/api/Account/{account_id}", auth_token)

def create_account(name: str, type: str, balance: float, auth_token: Optional[str]) -> Dict[str, Any]:
    """Yeni bir finansal hesap oluşturur."""
    logger.info(f"Python: create_account çağrıldı. Name: {name}, Type: {type}, Balance: {balance}")
    payload = {
        "name": name,
        "type": type, # AccountCreateDto'nuzdaki Type alanı string mi enum mı kontrol edin
        "balance": balance
    }
    return _make_api_request("/api/Account", auth_token, method="POST", json_data=payload)

# --- Araç ve Fonksiyon Eşleştirme Listeleri ---
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