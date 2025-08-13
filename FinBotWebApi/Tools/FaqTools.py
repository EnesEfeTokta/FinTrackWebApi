import json
import os
from typing import List, Dict, Optional

FAQ_FILE_PATH = os.path.join(os.path.dirname(__file__), '..', 'faq_data.json')

def get_application_faq(query: str) -> Dict:
    """
    Searches the application's knowledge base to find an answer to a user's question.
    
    :param query: The user's question about the application.
    :return: A dictionary containing the answer or a not found message.
    """
    try:
        with open(FAQ_FILE_PATH, 'r', encoding='utf-8') as f:
            faq_data: List[Dict] = json.load(f)
    except (FileNotFoundError, json.JSONDecodeError):
        return {"error": "Knowledge base is currently unavailable."}

    query_words = set(query.lower().split())
    best_match = None
    max_score = 0

    for item in faq_data:
        keywords = set(item.get("keywords", []))
        score = len(query_words.intersection(keywords))

        if score > max_score:
            max_score = score
            best_match = item

    if best_match and max_score > 0:
        return {
            "question_found": best_match["question"],
            "answer": best_match["answer"]
        }
    else:
        return {
            "answer": "I'm sorry, I couldn't find a specific answer to your question in my knowledge base. Could you try rephrasing it? I can help with topics like budgeting, security, and account management."
        }

FAQ_AVAILABLE_TOOLS = [
    {
        "name": "get_application_faq",
        "description": "Use this tool to answer user questions about how the FinTrack application works, its features, security, or general help topics. Use it when the user asks 'how to', 'what is', 'can I', or similar informational questions about the app itself, not about their personal data.",
        "arguments": {
            "query": {
                "type": "string",
                "description": "The user's specific question about the application."
            }
        }
    }
]

FAQ_FUNCTION_MAPPING = {
    "get_application_faq": get_application_faq
}