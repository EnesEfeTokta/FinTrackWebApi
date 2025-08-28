# **FinTrack API: Account Management (Account Controller)**

This document describes the `AccountController` endpoints used for managing users' financial accounts (`Account`).

*Controller Base Path:* `/Account`

---

## General Information

### Authentication

**All endpoints** in this controller require authorization. Requests must include a valid JWT `Bearer Token` in the `Authorization` header. The system identifies the user via the `userId` (from the NameIdentifier claim) within the token and performs operations only on behalf of that user.

**Header Example:**
`Authorization: Bearer <YOUR_JWT_TOKEN>`

An invalid or missing token will result in a `401 Unauthorized` error.

---

## Endpoints

### 1. Retrieve All of a User's Accounts

Lists all accounts registered in the system for the logged-in user.

*   **Endpoint:** `GET /Account`
*   **Description:** Returns a list of all accounts belonging to the authenticated user.
*   **Authorization:** Required (`User` or `Admin` role).

#### Success Response

*   **Status Code:** `200 OK`
*   **Content:** An array of `AccountDto` objects.
    ```json
    [
        {
          "id": 1,
          "name": "Salary Account",
          "type": "Bank",
          "isActive": true,
          "balance": 15250.75,
          "currency": "TRY",
          "createdAtUtc": "2024-05-20T10:00:00Z",
          "updatedAtUtc": "2024-05-23T14:30:00Z"
        },
        {
          "id": 2,
          "name": "Cash Wallet",
          "type": "Cash",
          "isActive": true,
          "balance": 850.00,
          "currency": "TRY",
          "createdAtUtc": "2024-05-21T11:00:00Z",
          "updatedAtUtc": "2024-05-22T18:00:00Z"
        }
    ]
    ```

#### Error Responses

*   **Status Code:** `500 Internal Server Error`
    ```json
    { "message": "An error occurred while retrieving accounts." }
    ```

---

### 2. Get a Specific Account

Retrieves the details of a single account belonging to the user by its ID.

*   **Endpoint:** `GET /Account/{Id}`
*   **Description:** Returns the details of the account with the given `Id` that belongs to the authenticated user.
*   **Authorization:** Required (`User` or `Admin` role).

#### URL Parameters

| Parameter | Type      | Description                   | Required? |
|-----------|-----------|-------------------------------|-----------|
| `Id`      | `integer` | The ID of the account to retrieve. | Yes       |

#### Success Response

*   **Status Code:** `200 OK`
*   **Content:** An `AccountDto` object.
    ```json
    {
      "id": 1,
      "name": "Salary Account",
      "type": "Bank",
      "isActive": true,
      "balance": 15250.75,
      "currency": "TRY",
      "createdAtUtc": "2024-05-20T10:00:00Z",
      "updatedAtUtc": "2024-05-23T14:30:00Z"
    }
    ```

#### Error Responses

*   **Status Code:** `404 Not Found`
    ```json
    { "message": "Account with ID 101 not found." }
    ```
*   **Status Code:** `500 Internal Server Error`

---

### 3. Create a New Account

Creates a new financial account for the user.

*   **Endpoint:** `POST /Account`
*   **Description:** Creates a new account based on the provided information and returns the created resource along with a `Location` header.
*   **Authorization:** Required (`User` or `Admin` role).

#### Request Body (`AccountCreateDto`)

*   **Content-Type:** `application/json`

| Field      | Type      | Description                               | Required? |
|------------|-----------|-------------------------------------------|-----------|
| `name`     | `string`  | The name of the account (e.g., "My Investment Account"). | Yes       |
| `type`     | `string`  | The account type. Possible values: `Cash`, `Bank`, `CreditCard`, `Investment`, `Other`. | Yes       |
| `isActive` | `boolean` | Whether the account is active.            | Yes       |
| `currency` | `string`  | The currency of the account. Possible values: `TRY`, `USD`, `EUR`, etc. | Yes       |

#### Request Body Example

```json
{
  "name": "Euro Account",
  "type": "Bank",
  "isActive": true,
  "currency": "EUR"
}
```

#### Success Response

*   **Status Code:** `201 Created`
*   **Headers:** `Location: /Account/{new_account_id}`
*   **Content:** The created `AccountModel` object.
    ```json
    {
        "id": 3,
        "userId": 15,
        "name": "Euro Account",
        "type": "Bank",
        "isActive": true,
        "balance": 0.0,
        "currency": "EUR",
        "createdAtUtc": "2024-05-24T12:00:00Z",
        "updatedAtUtc": null
    }
    ```

#### Error Responses

*   **Status Code:** `400 Bad Request`
*   **Status Code:** `500 Internal Server Error`

---

### 4. Update an Account

Updates an existing account.

*   **Endpoint:** `PUT /Account/{Id}`
*   **Description:** Updates the account with the specified `Id` using the provided data.
*   **Authorization:** Required (`User` or `Admin` role).

#### Request Body (`AccountUpdateDto`)

*   **Content-Type:** `application/json`

| Field    | Type      | Description                               | Required? |
|----------|-----------|-------------------------------------------|-----------|
| `name`   | `string`  | The new name of the account.              | Yes       |
| `type`   | `string`  | The account type. Possible values: `Cash`, `Bank`, `CreditCard`, `Investment`, `Other`. | Yes       |
| `currency`| `string` | The new currency of the account. Possible values: `TRY`, `USD`, `EUR`, etc. | Yes       |

#### Request Body Example
```json
{
  "name": "Dollar Investment Account",
  "type": "Investment",
  "currency": "USD"
}
```

#### Success Response

*   **Status Code:** `200 OK`
*   **Content:**
    ```json
    true
    ```

#### Error Responses

*   **Status Code:** `400 Bad Request`
*   **Status Code:** `404 Not Found`
*   **Status Code:** `500 Internal Server Error`

---

### 5. Delete an Account

Deletes an existing account.

*   **Endpoint:** `DELETE /Account/{Id}`
*   **Description:** Permanently deletes the account with the specified `Id`. This action cannot be undone.
*   **Authorization:** Required (`User` or `Admin` role).

#### Success Response

*   **Status Code:** `200 OK`
*   **Content:**
    ```json
    true
    ```

#### Error Responses

*   **Status Code:** `404 Not Found`
*   **Status Code:** `500 Internal Server Error`