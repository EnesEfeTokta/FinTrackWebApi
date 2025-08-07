import logging
from typing import List, Dict, Any, Optional
from decimal import Decimal
from ._api_helpers import _make_api_request

logger = logging.getLogger(__name__)

GET_TRANSACTION_CATEGORIES_TOOL = {
    "name": "get_transaction_categories",
    "description": "Lists all user-defined transaction categories (for both income and expense). Useful for seeing available categories before adding a transaction.",
    "parameters": {"type": "object", "properties": {}, "required": []}
}

CREATE_TRANSACTION_CATEGORY_TOOL = {
    "name": "create_transaction_category",
    "description": "Creates a new transaction category (e.g., 'Groceries', 'Consulting Income'). This is used if a category doesn't exist when adding a transaction.",
    "parameters": {
        "type": "OBJECT",
        "properties": {
            "name": {"type": "STRING", "description": "The name of the category to create."},
            "type": {"type": "STRING", "description": "The type of the category. Must be either 'Income' or 'Expense'."}
        },
        "required": ["name", "type"]
    }
}

CREATE_TRANSACTION_TOOL = {
    "name": "create_transaction",
    "description": "Records a new income or expense transaction. Before calling this, you should ensure the transaction's category exists by using 'get_transaction_categories' or create it with 'create_transaction_category'.",
    "parameters": {
        "type": "OBJECT",
        "properties": {
            "categoryId": {"type": "INTEGER", "description": "The ID of the category this transaction belongs to. This ID must be obtained from 'get_transaction_categories' or 'create_transaction_category'."},
            "accountId": {"type": "INTEGER", "description": "The ID of the account where the transaction occurred."},
            "amount": {"type": "NUMBER", "description": "The amount of the transaction."},
            "currency": {"type": "STRING", "description": "The currency of the transaction. e.g., 'TRY', 'USD', 'EUR'."},
            "transactionDateUtc": {"type": "STRING", "description": "The date of the transaction (YYYY-MM-DD format)."},
            "description": {"type": "STRING", "description": "A description for the transaction (optional)."}
        },
        "required": ["categoryId", "accountId", "amount", "currency", "transactionDateUtc"]
    }
}

GET_ALL_TRANSACTIONS_TOOL = {"name": "get_all_transactions", "description": "Lists all transactions.", "parameters": {}}
GET_TRANSACTIONS_BY_ACCOUNT_TOOL = {
    "name": "get_transactions_by_account_id",
    "description": "Lists all transactions for a specific account.",
    "parameters": {
        "type": "OBJECT", "properties": {"accountId": {"type": "INTEGER", "description": "The ID of the account."}}, "required": ["accountId"]
    }
}

def get_transaction_categories(auth_token: Optional[str]) -> List[Dict[str, Any]]:
    logger.info("Python: get_transaction_categories called.")
    return _make_api_request("/TransactionCategory", auth_token)

def create_transaction_category(name: str, type: str, auth_token: Optional[str]) -> Dict[str, Any]:
    logger.info(f"Python: create_transaction_category called. Name: {name}, Type: {type}")
    if type.lower() not in ['income', 'expense']:
        return {"error": "Invalid category type. Must be 'Income' or 'Expense'."}
    payload = {"name": name, "type": type.capitalize()}
    return _make_api_request("/TransactionCategory", auth_token, method="POST", json_data=payload)

def create_transaction(categoryId: int, accountId: int, amount: Decimal, currency: str, transactionDateUtc: str, auth_token: Optional[str], description: Optional[str] = None) -> Dict[str, Any]:
    logger.info(f"Python: create_transaction called. CategoryID: {categoryId}, AccountID: {accountId}, Amount: {amount}")
    payload = {"categoryId": categoryId, "accountId": accountId, "amount": amount, "currency": currency.upper(), "transactionDateUtc": transactionDateUtc, "description": description}
    return _make_api_request("/Transactions", auth_token, method="POST", json_data=payload)

def get_all_transactions(auth_token: Optional[str]) -> List[Dict[str, Any]]:
    logger.info("Python: get_all_transactions called.")
    return _make_api_request("/Transactions", auth_token)

def get_transactions_by_account_id(accountId: int, auth_token: Optional[str]) -> List[Dict[str, Any]]:
    logger.info(f"Python: get_transactions_by_account_id called. Account ID: {accountId}")
    return _make_api_request(f"/Transactions/account-id/{accountId}", auth_token)

TRANSACTION_AVAILABLE_TOOLS = [GET_TRANSACTION_CATEGORIES_TOOL, CREATE_TRANSACTION_CATEGORY_TOOL, CREATE_TRANSACTION_TOOL, GET_ALL_TRANSACTIONS_TOOL, GET_TRANSACTIONS_BY_ACCOUNT_TOOL]
TRANSACTION_FUNCTION_MAPPING = {"get_transaction_categories": get_transaction_categories, "create_transaction_category": create_transaction_category, "create_transaction": create_transaction, "get_all_transactions": get_all_transactions, "get_transactions_by_account_id": get_transactions_by_account_id}