import logging
from typing import List, Dict, Any, Optional
from decimal import Decimal
from ._api_helpers import _make_api_request

logger = logging.getLogger(__name__)

GET_BUDGETS_TOOL = {
    "name": "get_budgets",
    "description": "Lists all of the user's budgets.",
    "parameters": {"type": "OBJECT", "properties": {}, "required": []}
}

GET_BUDGET_DETAILS_TOOL = {
    "name": "get_budget_details",
    "description": "Fetches the details of a specific budget by its ID.",
    "parameters": {
        "type": "OBJECT",
        "properties": {"budgetId": {"type": "INTEGER", "description": "The ID of the budget to view."}},
        "required": ["budgetId"]
    }
}

CREATE_BUDGET_TOOL = {
    "name": "create_budget",
    "description": "Creates a new budget. If the specified category does not exist, it will be created automatically.",
    "parameters": {
        "type": "OBJECT",
        "properties": {
            "name": {"type": "STRING", "description": "The name of the budget (e.g., 'Monthly Expenses', 'Vacation Fund')."},
            "category": {"type": "STRING", "description": "The category for the budget (e.g., 'Food', 'Transport', 'Entertainment')."},
            "allocatedAmount": {"type": "NUMBER", "description": "The total amount allocated to the budget."},
            "currency" : {"type": "STRING", "description": "The currency of the budget. e.g., 'TRY', 'USD', 'EUR'."},
            "startDate": {"type": "STRING", "description": "The start date of the budget (YYYY-MM-DD format)."},
            "endDate": {"type": "STRING", "description": "The end date of the budget (YYYY-MM-DD format)."},
            "description": {"type": "STRING", "description": "A short description for the budget (optional)."},
            "isActive": {"type": "BOOLEAN", "description": "Indicates if the budget is active (optional, defaults to true)."}
        },
        "required": ["name", "category", "allocatedAmount", "currency", "startDate", "endDate"]
    }
}

GET_CATEGORIES_TOOL = {
    "name": "get_categories",
    "description": "Lists all available spending and income categories defined by the user.",
    "parameters": {"type": "OBJECT", "properties": {}, "required": []}
}

def get_budgets(auth_token: Optional[str]) -> List[Dict[str, Any]]:
    """Fetches all budgets for the user."""
    logger.info("Python: get_budgets called.")
    return _make_api_request("/Budgets", auth_token)

def get_budget_details(budgetId: int, auth_token: Optional[str]) -> Dict[str, Any]:
    """Fetches the details of a specific budget."""
    logger.info(f"Python: get_budget_details called. BudgetID: {budgetId}")
    return _make_api_request(f"/Budgets/{budgetId}", auth_token)

def create_budget(
    name: str, 
    category: str,
    allocatedAmount: Decimal,
    currency: str,
    startDate: str, 
    endDate: str, 
    auth_token: Optional[str],
    description: Optional[str] = None, 
    isActive: bool = True
) -> Dict[str, Any]:
    """Creates a new budget. The API handles category creation implicitly."""
    logger.info(f"Python: create_budget called. Name: {name}, Category: {category}")
    payload = {
        "name": name,
        "description": description,
        "category": category,
        "allocatedAmount": allocatedAmount,
        "currency": currency.upper(),
        "startDate": startDate,
        "endDate": endDate,
        "isActive": isActive
    }
    return _make_api_request("/Budgets", auth_token, method="POST", json_data=payload)

def get_categories(auth_token: Optional[str]) -> List[Dict[str, Any]]:
    """Fetches all categories from the CategoriesController."""
    logger.info("Python: get_categories called.")
    return _make_api_request("/Categories", auth_token)


BUDGET_AVAILABLE_TOOLS = [
    GET_BUDGETS_TOOL,
    GET_BUDGET_DETAILS_TOOL,
    CREATE_BUDGET_TOOL,
    GET_CATEGORIES_TOOL,
]

BUDGET_FUNCTION_MAPPING = {
    "get_budgets": get_budgets,
    "get_budget_details": get_budget_details,
    "create_budget": create_budget,
    "get_categories": get_categories,
}