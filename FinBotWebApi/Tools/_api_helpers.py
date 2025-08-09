import json
import logging
import os
from typing import List, Dict, Any, Optional
import requests
from decimal import Decimal

logger = logging.getLogger(__name__)
FINTRACK_API_BASE_URL = os.getenv("FINTRACK_API_BASE_URL", "http://localhost:8090")

class DecimalEncoder(json.JSONEncoder):
    def default(self, obj):
        if isinstance(obj, Decimal):
            return str(obj)
        return super(DecimalEncoder, self).default(obj)

def _make_api_request(endpoint: str, auth_token: Optional[str], method: str = "GET", params: Optional[Dict] = None, json_data: Optional[Dict] = None) -> Dict[str, Any] | List[Dict[str, Any]]:
    if not auth_token:
        logger.error("Auth Token not provided for API request. Endpoint: %s", endpoint)
        return {"error": "Authentication token is missing."}

    headers = {"Authorization": f"Bearer {auth_token}", "Accept": "application/json"}
    serialized_data = None
    if method.upper() in ["POST", "PUT"] and json_data is not None:
        headers["Content-Type"] = "application/json"
        serialized_data = json.dumps(json_data, cls=DecimalEncoder)

    url = f"{FINTRACK_API_BASE_URL}{endpoint}"
    
    try:
        logger.info(f"Python: Sending request to API: {method} {url}, Data: {serialized_data or json_data}")
        response = requests.request(method=method.upper(), url=url, headers=headers, params=params, data=serialized_data, timeout=15)
        
        if response.status_code == 204:
            logger.info(f"API returned 204 No Content. Endpoint: {url}")
            return {"message": "Operation completed successfully."} if method.upper() == "DELETE" else {}
            
        response.raise_for_status()
        return response.json()
    except requests.exceptions.HTTPError as http_err:
        logger.error(f"Python: API HTTP Error: {http_err} - Response: {getattr(http_err.response, 'text', 'No response')}")
        try:
            return {"error": f"API Error: {http_err.response.status_code}", "details": http_err.response.json()}
        except ValueError:
            return {"error": f"API Error: {http_err.response.status_code}", "details": http_err.response.text}
    except requests.exceptions.RequestException as req_err:
        logger.error(f"Python: API Request Error: {req_err}")
        return {"error": f"Unable to reach API: {req_err}"}
    except Exception as e:
        logger.error(f"Python: General error in API request: {e}", exc_info=True)
        return {"error": f"An unknown error occurred: {e}"}