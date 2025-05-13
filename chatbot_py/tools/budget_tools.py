# chatbot_py/tools/budget_tools.py
import logging
import os
from typing import List, Dict, Any, Optional
import requests
from dotenv import load_dotenv

load_dotenv()

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

FINTRACK_API_BASE_URL = os.getenv("FINTRACK_API_BASE_URL", "http://localhost:5246") # .env'den oku (portu kontrol edin)

# --- Yardımcı Fonksiyon (FinTrack API'sine istek atmak için - finance_tools.py'den kopyalanabilir veya ortak bir utils'e taşınabilir) ---
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

        # DELETE için 204 No Content başarılı bir yanıt olabilir, bu durumda boş dict dönelim.
        if response.status_code == 204 and method.upper() == "DELETE":
            logger.info(f"FinTrack API'den {method} isteği için 204 No Content yanıtı alındı. Endpoint: {url}")
            return {"message": "İşlem başarıyla silindi."} # Veya uygun bir başarı mesajı
            
        response.raise_for_status()
        
        if response.status_code == 204: # Diğer 204 durumları için
             logger.info(f"FinTrack API'den 204 No Content yanıtı alındı. Endpoint: {url}")
             return {} if method.upper() != "GET" or "history" not in endpoint else []
        
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
GET_USER_BUDGETS_TOOL = {
    "name": "get_user_budgets",
    "description": "Kullanıcının FinTrack sistemindeki tüm bütçelerini listeler.",
    "parameters": {
        "type": "OBJECT",
        "properties": {}, # Parametre yok, auth_token ile kullanıcı bilgisi alınacak
        "required": []
    }
}

GET_BUDGET_DETAILS_TOOL = {
    "name": "get_budget_details",
    "description": "Kullanıcının belirli bir bütçesinin detaylarını (ID ile) getirir.",
    "parameters": {
        "type": "OBJECT",
        "properties": {
            "budget_id": {
                "type": "INTEGER",
                "description": "Detayları görüntülenecek bütçenin ID'si."
            }
        },
        "required": ["budget_id"]
    }
}

CREATE_BUDGET_TOOL = {
    "name": "create_budget",
    "description": "Kullanıcı için yeni bir bütçe oluşturur.",
    "parameters": {
        "type": "OBJECT",
        "properties": {
            "name": {"type": "STRING", "description": "Bütçenin adı (örneğin 'Aylık Harcamalar', 'Tatil Fonu')."},
            "description": {"type": "STRING", "description": "Bütçe için kısa bir açıklama (isteğe bağlı)."},
            "start_date": {"type": "STRING", "description": "Bütçenin başlangıç tarihi (YYYY-AA-GG formatında)."},
            "end_date": {"type": "STRING", "description": "Bütçenin bitiş tarihi (YYYY-AA-GG formatında)."},
            "is_active": {"type": "BOOLEAN", "description": "Bütçenin aktif olup olmadığı (varsayılan: true)."}
        },
        "required": ["name", "start_date", "end_date"]
    }
}

# Şimdilik güncelleme ve silme fonksiyonlarını eklemeyelim, LLM'in bunları yanlışlıkla tetiklemesi riskli olabilir.
# Bunlar daha çok kullanıcı arayüzünden onay alınarak yapılmalı.
# Ancak LLM, "bütçemi güncelle" veya "şu bütçeyi sil" gibi bir istek geldiğinde kullanıcıyı
# doğru sayfaya yönlendirebilir veya bu işlemin nasıl yapılacağını anlatabilir.

# --- Python Fonksiyonları ---
def get_user_budgets(auth_token: Optional[str]) -> List[Dict[str, Any]]:
    """Kullanıcının tüm bütçelerini FinTrack API'sinden alır."""
    logger.info(f"Python: get_user_budgets çağrıldı.")
    result = _make_api_request("/api/Budgets/budgets", auth_token)
    return result if isinstance(result, list) else [result] if isinstance(result, dict) and "error" in result else []

def get_budget_details(budget_id: int, auth_token: Optional[str]) -> Dict[str, Any]:
    """Belirli bir bütçenin detaylarını FinTrack API'sinden alır."""
    logger.info(f"Python: get_budget_details çağrıldı. BudgetID: {budget_id}")
    return _make_api_request(f"/api/Budgets/{budget_id}", auth_token)

def create_budget(name: str, start_date: str, end_date: str, description: Optional[str] = None, is_active: bool = True, auth_token: Optional[str] = None) -> Dict[str, Any]:
    """Yeni bir bütçe oluşturur."""
    logger.info(f"Python: create_budget çağrıldı. Name: {name}, Start: {start_date}, End: {end_date}")
    payload = {
        "name": name,
        "description": description,
        "startDate": start_date, # Tarih formatlarının API'nin beklediği gibi olduğundan emin olun
        "endDate": end_date,
        "isActive": is_active
    }
    return _make_api_request("/api/Budgets", auth_token, method="POST", json_data=payload)


# --- Araç ve Fonksiyon Eşleştirme Listeleri ---
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