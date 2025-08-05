import logging
from typing import List, Dict, Any, Optional
from ._api_helpers import _make_api_request

logger = logging.getLogger(__name__)

GET_USER_ACCOUNTS_TOOL = {
    "name": "get_user_accounts",
    "description": "Lists all of the user's financial accounts (e.g., bank accounts, wallets) and their current balances.",
    "parameters": {"type": "OBJECT", "properties": {}, "required": []}
}

GET_ACCOUNT_DETAILS_TOOL = {
    "name": "get_account_details",
    "description": "Fetches the details and balance of a specific financial account using its ID.",
    "parameters": {
        "type": "OBJECT",
        "properties": {"account_id": {"type": "INTEGER", "description": "The ID of the account to view."}},
        "required": ["account_id"]
    }
}

CREATE_ACCOUNT_TOOL = {
    "name": "create_account",
    "description": "Creates a new financial account for the user (e.g., a bank account, cash wallet). The account is created with a zero balance.",
    "parameters": {
        "type": "OBJECT",
        "properties": {
            "name": {"type": "STRING", "description": "The name of the account to create (e.g., 'My Salary Account', 'Cash Wallet')."},
            "type": {"type": "STRING", "description": "The type of the account. Valid values: 'Checking', 'Savings', 'CreditCard', 'Cash', 'Investment', 'Loan', 'Other'."},
            "currency": {"type": "STRING", "description": "The currency of the account. Valid values: 'TRY', 'USD', 'EUR', 'GBP', etc."},
            "is_active": {"type": "BOOLEAN", "description": "Indicates if the account is active. Defaults to True and is optional."}
        },
        "required": ["name", "type", "currency"]
    }
}

def get_user_accounts(auth_token: Optional[str]) -> List[Dict[str, Any]]:
    logger.info("Python: get_user_accounts called.")
    result = _make_api_request("/Account", auth_token)
    return result if isinstance(result, list) else [result] if isinstance(result, dict) and "error" in result else []

def get_account_details(account_id: int, auth_token: Optional[str]) -> Dict[str, Any]:
    logger.info(f"Python: get_account_details called. AccountID: {account_id}")
    return _make_api_request(f"/Account/{account_id}", auth_token)

def create_account(name: str, type: str, currency: str, auth_token: Optional[str], is_active: bool = True) -> Dict[str, Any]:
    logger.info(f"Python: create_account called. Name: {name}, Type: {type}, IsActive: {is_active}")
    payload = {"name": name, "type": type.upper(), "is_active": is_active, "currency": currency.upper()}
    return _make_api_request("/Account", auth_token, method="POST", json_data=payload)

ACCOUNT_AVAILABLE_TOOLS = [GET_USER_ACCOUNTS_TOOL, GET_ACCOUNT_DETAILS_TOOL, CREATE_ACCOUNT_TOOL]
ACCOUNT_FUNCTION_MAPPING = {"get_user_accounts": get_user_accounts, "get_account_details": get_account_details, "create_account": create_account}