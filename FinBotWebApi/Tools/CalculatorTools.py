import logging
from typing import List, Dict, Any
from decimal import Decimal, InvalidOperation

logger = logging.getLogger(__name__)

CALCULATOR_TOOL = {
    "name": "calculate_sum",
    "description": "Calculates the sum of a list of numbers. Use this to add up incomes, expenses, or any other values. Can also be used to subtract by providing negative numbers.",
    "parameters": {
        "type": "OBJECT",
        "properties": {
            "values": {
                "type": "ARRAY",
                "items": {"type": "NUMBER"},
                "description": "A list of numbers (as strings or numbers) to be added together. For subtraction, use negative numbers (e.g., [100, -25.50])."
            }
        },
        "required": ["values"]
    }
}

def calculate_sum(values: List[Any]) -> Dict[str, Any]:
    logger.info(f"Python: calculate_sum called with values: {values}")
    total = Decimal('0')
    try:
        for value in values:
            total += Decimal(str(value))
        result = {"total": total}
        logger.info(f"Calculation result: {result}")
        return result
    except (InvalidOperation, TypeError) as e:
        logger.error(f"Error during calculation: {e}")
        return {"error": "Invalid data provided. Please provide a list of numbers."}

CALCULATOR_AVAILABLE_TOOLS = [CALCULATOR_TOOL]
CALCULATOR_FUNCTION_MAPPING = {"calculate_sum": calculate_sum}