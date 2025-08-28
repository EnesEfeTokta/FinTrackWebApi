# **FinTrack API: Transaction Management (Transactions Controller)**

This document describes the `TransactionsController` endpoints used for recording, viewing, updating, and deleting users' financial transactions (income/expenses).

*Controller Base Path:* `/Transactions`

---

## General Information

### Authentication

**All endpoints** in this controller require authorization. Requests must include a valid JWT `Bearer Token` in the `Authorization` header.

### Automatic Balance Update

This controller automatically updates the balance of the associated account (`Account`) when a transaction is created or deleted.
*   **Creating a Transaction:**
    *   If the transaction is of type `Income`, the account balance is increased.
    *   If the transaction is of type `Expense`, the account balance is decreased.
*   **Deleting a Transaction:**
    *   If an `Income` transaction is deleted, the account balance is decreased (reverted).
    *   If an `Expense` transaction is deleted, the account balance is increased (reverted).

---

## Endpoints

### 1. Get All of a User's Transactions

Lists all transactions in the system for the logged-in user.

*   **Endpoint:** `GET /Transactions`
*   **Description:** Returns all income and expense records belonging to the user.
*   **Authorization:** Required (`User` or `Admin` role).

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** An array of `TransactionDto` objects.
    ```json
    [
        {
          "id": 1,
          "category": { "id": 1, "name": "Salary", "type": "Income" /*...*/ },
          "account": { "id": 1, "name": "Salary Account", "balance": 15000 /*...*/ },
          "amount": 25000,
          "currency": "TRY",
          "transactionDateUtc": "2024-05-01T08:00:00Z",
          "description": "May Salary",
          "createdAtUtc": "2024-05-01T08:01:00Z",
          "updatedAtUtc": null
        }
    ]
    ```

### 2. Get a Specific Transaction

Retrieves the details of a single transaction belonging to the user by its ID.

*   **Endpoint:** `GET /Transactions/{Id}`
*   **Authorization:** Required.

### 3. Filter Transactions by Category Type

Filters a user's transactions by category type (`Income` or `Expense`).

*   **Endpoint:** `GET /Transactions/category-type/{type}`
*   **URL Parameter:** `type` (string) - Values: `Income`, `Expense`.
*   **Authorization:** Required.

### 4. Filter Transactions by Category Name

Filters a user's transactions by a specific category name.

*   **Endpoint:** `GET /Transactions/category-name/{category}`
*   **URL Parameter:** `category` (string) - The full name of the category (e.g., "Bills").
*   **Authorization:** Required.

### 5. Filter Transactions by Account

Filters a user's transactions by a specific account ID.

*   **Endpoint:** `GET /Transactions/account-id/{account}`
*   **URL Parameter:** `account` (integer) - The ID of the account.
*   **Authorization:** Required.

### 6. Create a New Transaction

Creates a new income or expense record for the user.

*   **Endpoint:** `POST /Transactions`
*   **Description:** As a result of this operation, the balance of the associated account is automatically updated.
*   **Authorization:** Required.

#### Request Body (`TransactionCreateDto`)
| Field | Type | Description | Required? |
| :--- | :--- | :--- | :--- |
| `categoryId` | `integer`| The ID of the associated category. | Yes |
| `accountId` | `integer`| The ID of the account where the transaction occurred. | Yes |
| `amount` | `number` | The transaction amount (positive for income, negative for expense).| Yes |
| `currency` | `string` | The currency (e.g., "TRY"). Must match the account's currency. | Yes |
| `transactionDateUtc` | `string` | The date of the transaction (ISO 8601). | Yes |
| `description` | `string` | A description of the transaction. | No |

#### Success Response
*   **Status Code:** `201 Created`
*   **Content:** The created `TransactionModel` object.

### 7. Update a Transaction

Updates the details of an existing transaction. **Note:** This operation does **not** automatically update the account balance. Balance adjustments must be made manually.

*   **Endpoint:** `PUT /Transactions/{Id}`
*   **Authorization:** Required.

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** `true`

### 8. Delete a Transaction

Deletes an existing transaction.

*   **Endpoint:** `DELETE /Transactions/{Id}`
*   **Description:** As a result of this operation, the balance of the associated account is automatically updated (the transaction is reverted).
*   **Authorization:** Required.

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** `true`