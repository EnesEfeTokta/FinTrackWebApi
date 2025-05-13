import logging
import os
from typing import List, Dict, Any, Optional
import requests # HTTP istekleri için
from dotenv import load_dotenv

load_dotenv() # .env dosyasındaki değişkenleri yükle

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

FINTRACK_API_BASE_URL = os.getenv("FINTRACK_API_BASE_URL", "http://localhost:5246") # .env'den oku

# Fonksiyon Şemaları (Tool Tanımları) - Bunlar aynı kalabilir, sadece parametre açıklamaları güncellenebilir.
GET_ALL_TRANSACTIONS_TOOL = {
    "name": "get_all_user_transactions", # Fonksiyon adını biraz daha spesifik yapabiliriz
    "description": "Kullanıcının FinTrack sistemindeki tüm gelir ve gider işlemlerini listeler.",
    "parameters": {
        "type": "OBJECT",
        "properties": {
            # user_id artık LLM tarafından değil, context'ten (auth_token ile) alınacak.
            # Ancak LLM'e hangi kullanıcı için işlem yaptığını belirtmek için user_id'yi
            # sistem mesajında veya başka bir yolla verebiliriz.
            # Şimdilik, fonksiyonun bunu dışarıdan almadığını varsayalım, auth_token kullanılacak.
        },
        "required": [] # user_id'yi kaldırdık, çünkü token'dan gelecek
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

GET_TRANSACTIONS_BY_CATEGORY_NAME_TOOL = { # Yeni şema
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

# --- Python Fonksiyonları (Gerçek API Çağrıları ile) ---

def _make_fin_track_api_request(endpoint: str, auth_token: Optional[str], method: str = "GET", params: Optional[Dict] = None, json_data: Optional[Dict] = None) -> List[Dict[str, Any]]:
    """Helper function to make requests to FinTrack API."""
    if not auth_token:
        logger.error("FinTrack API isteği için Auth Token sağlanmadı.")
        # Kullanıcıya token'ının eksik olduğunu bildiren bir mesaj döndürebiliriz
        # veya bir exception fırlatabiliriz.
        return [{"error": "Kimlik doğrulama bilgisi eksik."}]

    headers = {
        "Authorization": f"Bearer {auth_token}",
        "Accept": "application/json"
    }
    url = f"{FINTRACK_API_BASE_URL}{endpoint}"
    
    try:
        logger.info(f"Python: FinTrack API'sine istek gönderiliyor: {method} {url}, Params: {params}")
        if method.upper() == "GET":
            response = requests.get(url, headers=headers, params=params, timeout=10)
        elif method.upper() == "POST":
            headers["Content-Type"] = "application/json"
            response = requests.post(url, headers=headers, json=json_data, timeout=10)
        # Diğer metotlar (PUT, DELETE) da eklenebilir
        else:
            logger.error(f"Desteklenmeyen HTTP metodu: {method}")
            return [{"error": f"Desteklenmeyen HTTP metodu: {method}"}]

        response.raise_for_status() # 4xx veya 5xx hatalarında exception fırlatır
        return response.json()
    except requests.exceptions.HTTPError as http_err:
        logger.error(f"Python: FinTrack API HTTP Hatası: {http_err} - Yanıt: {http_err.response.text}")
        try:
            error_detail = http_err.response.json()
            return [{"error": f"API Hatası: {http_err.response.status_code}", "details": error_detail}]
        except ValueError: # JSON parse hatası
            return [{"error": f"API Hatası: {http_err.response.status_code}", "details": http_err.response.text}]
    except requests.exceptions.RequestException as req_err:
        logger.error(f"Python: FinTrack API İstek Hatası: {req_err}")
        return [{"error": f"FinTrack API'sine ulaşılamadı: {req_err}"}]
    except Exception as e:
        logger.error(f"Python: FinTrack API isteğinde genel hata: {e}", exc_info=True)
        return [{"error": f"Bilinmeyen bir hata oluştu: {e}"}]


def get_all_user_transactions(auth_token: Optional[str]) -> List[Dict[str, Any]]:
    """
    Kullanıcının FinTrack sistemindeki tüm gelir ve gider işlemlerini listeler.
    auth_token: Kullanıcının JWT'si.
    """
    logger.info(f"Python: get_all_user_transactions çağrıldı.")
    return _make_fin_track_api_request("/api/Transactions", auth_token)

def get_user_transactions_by_category_type(category_type: str, auth_token: Optional[str]) -> List[Dict[str, Any]]:
    """
    Kullanıcının belirli bir kategori türündeki tüm işlemlerini listeler.
    category_type: 'Gelir' veya 'Gider'.
    auth_token: Kullanıcının JWT'si.
    """
    logger.info(f"Python: get_user_transactions_by_category_type çağrıldı. CategoryType: {category_type}")
    # ASP.NET API endpoint'inizin tam yolu: /api/Transactions/category-type/{type}
    return _make_fin_track_api_request(f"/api/Transactions/category-type/{category_type}", auth_token)

def get_user_transactions_by_category_name(category_name: str, auth_token: Optional[str]) -> List[Dict[str, Any]]:
    """
    Kullanıcının belirli bir kategori adına sahip tüm işlemlerini listeler.
    category_name: Örneğin 'Maaş', 'Market'.
    auth_token: Kullanıcının JWT'si.
    """
    logger.info(f"Python: get_user_transactions_by_category_name çağrıldı. CategoryName: {category_name}")
    # ASP.NET API endpoint'inizin tam yolu: /api/Transactions/category-name/{categoryName}
    return _make_fin_track_api_request(f"/api/Transactions/category-name/{category_name}", auth_token)


FUNCTION_MAPPING = {
    "get_all_user_transactions": get_all_user_transactions,
    "get_user_transactions_by_category_type": get_user_transactions_by_category_type,
    "get_user_transactions_by_category_name": get_user_transactions_by_category_name,
}