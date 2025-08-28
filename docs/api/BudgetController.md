# **FinTrack API: Budget Management (Budgets Controller)**

This document describes the `BudgetsController` endpoints used for creating, tracking, and managing users' financial budgets.

*Controller Base Path:* `/Budgets`

---

## General Information

### Authentication

**All endpoints** in this controller require authorization. Requests must include a valid JWT `Bearer Token` in the `Authorization` header. The system identifies the user via the `userId` from the token and performs operations only on behalf of that user.

**Header Example:**
`Authorization: Bearer <YOUR_JWT_TOKEN>`

### Dynamic Category Management

During budget creation or updates, if the specified `category` name does not exist among the user's current categories, the system will **automatically create** that category for the user. This allows users to define new spending categories on the fly while creating a budget.

---

## Endpoints

### 1. Retrieve All of a User's Budgets

Lists all budgets registered in the system for the logged-in user.

*   **Endpoint:** `GET /Budgets`
*   **Description:** Returns a list of all budgets belonging to the authenticated user.
*   **Authorization:** Required (`User` or `Admin` role).

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** An array of `BudgetDto` objects.
    ```json
    [
        {
          "id": 1,
          "name": "Monthly Grocery Shopping",
          "description": "For essential food and cleaning supplies.",
          "category": "Groceries",
          "allocatedAmount": 5000.00,
          "reachedAmount": 2350.50,
          "currency": "TRY",
          "startDate": "2024-05-01T00:00:00Z",
          "endDate": "2024-05-31T23:59:59Z",
          "isActive": true,
          "createdAtUtc": "2024-05-01T10:00:00Z",
          "updatedAtUtc": "2024-05-20T15:00:00Z"
        }
    ]
    ```

#### Error Responses
*   **Status Code:** `500 Internal Server Error`

---

### 2. Get a Specific Budget

Retrieves the details of a single budget belonging to the user by its ID.

*   **Endpoint:** `GET /Budgets/{id}`
*   **Description:** Returns the details of the budget with the given `id` that belongs to the authenticated user.
*   **Authorization:** Required (`User` or `Admin` role).

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** A `BudgetDto` object.
    ```json
    {
      "id": 1,
      "name": "Monthly Grocery Shopping",
      "description": "For essential food and cleaning supplies.",
      "category": "Groceries",
      "allocatedAmount": 5000.00,
      "reachedAmount": 2350.50,
      "currency": "TRY",
      "startDate": "2024-05-01T00:00:00Z",
      "endDate": "2024-05-31T23:59:59Z",
      "isActive": true,
      "createdAtUtc": "2024-05-01T10:00:00Z",
      "updatedAtUtc": "2024-05-20T15:00:00Z"
    }
    ```

#### Error Responses
*   **Status Code:** `404 Not Found`
*   **Status Code:** `500 Internal Server Error`

---

### 3. Create a New Budget

Creates a new financial budget for the user.

*   **Endpoint:** `POST /Budgets`
*   **Description:** Creates a new budget based on the provided information. If the specified category does not exist, it will be created automatically.
*   **Authorization:** Required (`User` or `Admin` role).

#### Request Body (`BudgetCreateDto`)
| Field | Type | Description | Required? |
| :--- | :--- | :--- | :--- |
| `name` | `string` | The name of the budget. | Yes |
| `description`| `string` | A short description of the budget. | No |
| `category` | `string` | The name of the category associated with the budget. | Yes |
| `allocatedAmount` | `number` | The total amount allocated for this budget. | Yes |
| `reachedAmount`| `number`| The initial spent amount for the budget (usually 0).| Yes |
| `currency` | `string` | The currency (e.g., "TRY", "USD"). | Yes |
| `startDate` | `string` | The start date of the budget (in ISO 8601 format). | Yes |
| `endDate` | `string` | The end date of the budget (in ISO 8601 format). | Yes |
| `isActive` | `boolean` | Whether the budget is active. | Yes |

#### Request Body Example
```json
{
  "name": "Dining Out Budget",
  "category": "Restaurant & Cafe",
  "allocatedAmount": 1500,
  "reachedAmount": 0,
  "currency": "TRY",
  "startDate": "2024-06-01T00:00:00Z",
  "endDate": "2024-06-30T23:59:59Z",
  "isActive": true
}
```

#### Success Response
*   **Status Code:** `201 Created`
*   **Content:** The created budget as a `BudgetDto` object.

---

### 4. Update a Budget

Updates an existing budget.

*   **Endpoint:** `PUT /Budgets/{id}`
*   **Description:** Updates the budget with the specified `id` using the provided data.
*   **Authorization:** Required (`User` or `Admin` role).

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** The updated budget as a `BudgetDto` object.

#### Error Responses
*   **Status Code:** `404 Not Found`
*   **Status Code:** `500 Internal Server Error`

---

### 5. Update a Budget's Reached Amount

A dedicated endpoint to update only the spent (`reachedAmount`) value of a budget.

*   **Endpoint:** `PUT /Budgets/Update-Reached-Amount`
*   **Description:** Typically used to update the current spending amount of a related budget when a transaction is added or deleted.
*   **Authorization:** Required (`User` or `Admin` role).

#### Request Body (`BudgetUpdateReachedAmountDto`)
| Field | Type | Description | Required? |
| :--- | :--- | :--- | :--- |
| `budgetId` | `integer`| The ID of the budget to be updated. | Yes |
| `reachedAmount`|`number`| The new total spent amount for the budget. | Yes |

#### Request Body Example
```json
{
  "budgetId": 1,
  "reachedAmount": 2500.75
}
```

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** The updated budget as a `BudgetDto` object.

#### Error Responses
*   **Status Code:** `400 Bad Request`
*   **Status Code:** `404 Not Found`
*   **Status Code:** `500 Internal Server Error`

---

### 6. Delete a Budget

Deletes an existing budget.

*   **Endpoint:** `DELETE /Budgets/{id}`
*   **Description:** Permanently deletes the budget with the specified `id`. This action cannot be undone.
*   **Authorization:** Required (`User` or `Admin` role).

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** `true`

#### Error Responses
*   **Status Code:** `404 Not Found`
*   **Status Code:** `500 Internal Server Error`