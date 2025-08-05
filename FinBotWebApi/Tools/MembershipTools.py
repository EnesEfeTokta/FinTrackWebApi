import logging
from typing import List, Dict, Any, Optional
from ._api_helpers import _make_api_request

logger = logging.getLogger(__name__)

GET_AVAILABLE_MEMBERSHIP_PLANS_TOOL = {
    "name": "get_available_membership_plans",
    "description": "Lists all available membership plans that a user can subscribe to, including their features and prices.",
    "parameters": {"type": "OBJECT", "properties": {}, "required": []}
}

GET_CURRENT_USER_MEMBERSHIP_TOOL = {
    "name": "get_current_user_membership",
    "description": "Fetches the user's current active membership plan, including its status, name, and validity dates.",
    "parameters": {"type": "OBJECT", "properties": {}, "required": []}
}

GET_USER_MEMBERSHIP_HISTORY_TOOL = {
    "name": "get_user_membership_history",
    "description": "Lists all of the user's past memberships (active, expired, canceled).",
    "parameters": {"type": "OBJECT", "properties": {}, "required": []}
}

SUBSCRIBE_TO_PLAN_TOOL = {
    "name": "subscribe_to_plan",
    "description": "Subscribes the user to a specific membership plan using its ID. This will initiate the payment process.",
    "parameters": {
        "type": "OBJECT",
        "properties": {
            "planId": {"type": "INTEGER", "description": "The ID of the plan to subscribe to. This ID can be found using the 'get_available_membership_plans' tool."},
            "autoRenew": {"type": "BOOLEAN", "description": "Whether the subscription should automatically renew. Defaults to true."}
        },
        "required": ["planId"]
    }
}

CANCEL_SUBSCRIPTION_TOOL = {
    "name": "cancel_subscription",
    "description": "Cancels an active user membership subscription by its ID. The membership remains active until the end date.",
    "parameters": {
        "type": "OBJECT",
        "properties": {
            "userMembershipId": {"type": "INTEGER", "description": "The ID of the user's specific membership to cancel. This ID is obtained from 'get_current_user_membership' or 'get_user_membership_history'."}
        },
        "required": ["userMembershipId"]
    }
}

def get_available_membership_plans(auth_token: Optional[str]) -> List[Dict[str, Any]]:
    """Fetches all available and active membership plans."""
    logger.info("Python: get_available_membership_plans called.")
    return _make_api_request("/Membership/plans", auth_token)

def get_current_user_membership(auth_token: Optional[str]) -> Dict[str, Any]:
    """Retrieves the user's current active membership status."""
    logger.info("Python: get_current_user_membership called.")
    return _make_api_request("/Membership/current", auth_token)

def get_user_membership_history(auth_token: Optional[str]) -> List[Dict[str, Any]]:
    """Retrieves the user's entire membership history."""
    logger.info("Python: get_user_membership_history called.")
    result = _make_api_request("/Membership/history", auth_token)
    return result if isinstance(result, list) else [result] if "error" in result else []

def subscribe_to_plan(planId: int, auth_token: Optional[str], autoRenew: bool = True) -> Dict[str, Any]:
    """
    Initiates a subscription to a membership plan for the user.
    Corresponds to the /Membership/create-checkout-session endpoint.
    """
    logger.info(f"Python: subscribe_to_plan called. PlanID: {planId}, AutoRenew: {autoRenew}")
    payload = {
        "planId": planId,
        "autoRenew": autoRenew
    }
    return _make_api_request("/Membership/create-checkout-session", auth_token, method="POST", json_data=payload)

def cancel_subscription(userMembershipId: int, auth_token: Optional[str]) -> Dict[str, Any]:
    """Cancels a user's active subscription."""
    logger.info(f"Python: cancel_subscription called. UserMembershipID: {userMembershipId}")
    return _make_api_request(f"/Membership/{userMembershipId}/cancel", auth_token, method="POST")


MEMBERSHIP_AVAILABLE_TOOLS = [
    GET_AVAILABLE_MEMBERSHIP_PLANS_TOOL,
    GET_CURRENT_USER_MEMBERSHIP_TOOL,
    GET_USER_MEMBERSHIP_HISTORY_TOOL,
    SUBSCRIBE_TO_PLAN_TOOL,
    CANCEL_SUBSCRIPTION_TOOL,
]

MEMBERSHIP_FUNCTION_MAPPING = {
    "get_available_membership_plans": get_available_membership_plans,
    "get_current_user_membership": get_current_user_membership,
    "get_user_membership_history": get_user_membership_history,
    "subscribe_to_plan": subscribe_to_plan,
    "cancel_subscription": cancel_subscription,
}