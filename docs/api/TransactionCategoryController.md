# **FinTrack API: Transaction Category Management (TransactionCategory Controller)**

This document describes the `TransactionCategoryController` endpoints, which are used to manage the personal **transaction categories** that users employ to classify their income and expense transactions.

*Controller Base Path:* `/TransactionCategory`

---

## General Information

### Authentication

**All endpoints** in this controller require authorization. Requests must include a valid JWT `Bearer Token` in the `Authorization` header.

### Data Isolation (User Scoping)

All category operations are tied to the identity of the user performing the action. A user can only list, view, update, or delete the transaction categories they have created.

### `type` Field Values (`TransactionType`)

Transaction categories are classified as either `Income` or `Expense`. This allows users to create more meaningful groupings for budgeting and reporting.
*   `Income`
*   `Expense`

---

## Endpoints

### 1. Retrieve All of a User's Transaction Categories

Lists all transaction categories registered in the system for the logged-in user.

*   **Endpoint:** `GET /TransactionCategory`
*   **Description:** Returns a list of all transaction categories belonging to the authenticated user.
*   **Authorization:** Required (`User` or `Admin` role).

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** An array of `TransactionCategoriesDto` objects.
    ```json
    [
        {
          "id": 1,
          "name": "Salary",
          "type": "Income",
          "createdAt": "2024-05-01T10:00:00Z",
          "updatedAt": null
        },
        {
          "id": 2,
          "name": "Bills",
          "type": "Expense",
          "createdAt": "2024-05-02T11:30:00Z",
          "updatedAt": "2024-05-22T14:00:00Z"
        }
    ]
    ```

#### Error Responses
*   `404 Not Found`: If the user has no categories.
*   `500 Internal Server Error`

---

### 2. Get a Specific Transaction Category

Retrieves the details of a single transaction category belonging to the user by its ID.

*   **Endpoint:** `GET /TransactionCategory/{Id}`
*   **Description:** Returns the details of the transaction category with the given `Id` that belongs to the user.
*   **Authorization:** Required (`User` or `Admin` role).

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** A single `TransactionCategoriesDto` object.

#### Error Responses
*   `404 Not Found`
*   `500 Internal Server Error`

---

### 3. Create a New Transaction Category

Creates a new transaction category for the user.

*   **Endpoint:** `POST /TransactionCategory`
*   **Authorization:** Required (`User` or `Admin` role).

#### Request Body (`TransactionCategoriesCreateDto`)
| Field | Type | Description | Required? |
| :--- | :--- | :--- | :--- |
| `name` | `string` | The name of the category (e.g., "Transportation"). | Yes |
| `type` | `string` | The type of the category (`Income` or `Expense`). | Yes |

#### Request Body Example
```json
{
  "name": "Rent",
  "type": "Expense"
}
```

#### Success Response
*   **Status Code:** `201 Created`
*   **Content:** The created `TransactionCategoryModel` object.

---

### 4. Update a Transaction Category

Updates the name and type of an existing transaction category.

*   **Endpoint:** `PUT /TransactionCategory/{Id}`
*   **Authorization:** Required (`User` or `Admin` role).

#### Request Body (Uses `TransactionCategoriesCreateDto`)
| Field | Type | Description | Required? |
| :--- | :--- | :--- | :--- |
| `name` | `string` | The new name of the category. | Yes |
| `type` | `string` | The new type of the category (`Income` or `Expense`). | Yes |

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** `true`

#### Error Responses
*   `404 Not Found`
*   `500 Internal Server Error`

---

### 5. Delete a Transaction Category

Deletes an existing transaction category.

*   **Endpoint:** `DELETE /TransactionCategory/{Id}`
*   **Description:** This action cannot be undone. It should be noted that transactions associated with this category may become uncategorized.
*   **Authorization:** Required (`User` or `Admin` role).

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** `true`

#### Error Responses
*   `404 Not Found`
*   `500 Internal Server Error`