# **FinTrack API: Category Management (Categories Controller)**

This document describes the `CategoriesController` endpoints, which are used to manage the personal categories that users employ to classify their transactions (income/expenses).

*Controller Base Path:* `/Categories`

---

## General Information

### Authentication

**All endpoints** in this controller require authorization. Requests must include a valid JWT `Bearer Token` in the `Authorization` header.

**Header Example:**
`Authorization: Bearer <YOUR_JWT_TOKEN>`

### Data Isolation (User Scoping)

All category operations are tied to the identity of the user performing the action. A user can only list, view, update, or delete the categories they have created. Access to another user's data is not possible.

---

## Endpoints

### 1. Retrieve All of a User's Categories

Lists all income/expense categories registered in the system for the logged-in user.

*   **Endpoint:** `GET /Categories`
*   **Description:** Returns a list of all categories belonging to the authenticated user.
*   **Authorization:** Required (`User` or `Admin` role).

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** An array of `CategoryDto` objects. If the user has no categories, an empty array `[]` is returned.
    ```json
    [
        {
          "id": 1,
          "name": "Bills",
          "createdAtUtc": "2024-05-01T10:00:00Z",
          "updatedAtUtc": "2024-05-20T15:00:00Z"
        },
        {
          "id": 2,
          "name": "Groceries",
          "createdAtUtc": "2024-05-02T11:30:00Z",
          "updatedAtUtc": null
        }
    ]
    ```

#### Error Responses
*   **Status Code:** `500 Internal Server Error`

---

### 2. Get a Specific Category

Retrieves the details of a single category belonging to the user by its ID.

*   **Endpoint:** `GET /Categories/{categoryId}`
*   **Description:** Returns the details of the category with the given `categoryId` that belongs to the user.
*   **Authorization:** Required (`User` or `Admin` role).

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** A `CategoryDto` object.
    ```json
    {
      "id": 1,
      "name": "Bills",
      "createdAtUtc": "2024-05-01T10:00:00Z",
      "updatedAtUtc": "2024-05-20T15:00:00Z"
    }
    ```

#### Error Responses
*   **Status Code:** `404 Not Found` (If the category is not found or does not belong to the user).
*   **Status Code:** `500 Internal Server Error`

---

### 3. Create a New Category

Creates a new category for the user.

*   **Endpoint:** `POST /Categories`
*   **Description:** Creates a new category based on the provided `name`.
*   **Authorization:** Required (`User` or `Admin` role).

#### Request Body (`CategoryCreateDto`)
| Field | Type | Description | Required? |
| :--- | :--- | :--- | :--- |
| `name` | `string` | The name of the category (e.g., "Transportation"). | Yes |

#### Request Body Example
```json
{
  "name": "Entertainment"
}
```

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:**
    ```json
    true
    ```

#### Error Responses
*   **Status Code:** `500 Internal Server Error`

---

### 4. Update a Category

Updates the name of an existing category.

*   **Endpoint:** `PUT /Categories/{categoryId}`
*   **Description:** Updates the category with the specified `categoryId` with the new `name` provided.
*   **Authorization:** Required (`User` or `Admin` role).

#### Request Body (`CategoryUpdateDto`)
| Field | Type | Description | Required? |
| :--- | :--- | :--- | :--- |
| `name` | `string` | The new name of the category. | Yes |

#### Request Body Example
```json
{
  "name": "Subscriptions & Dues"
}
```

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** The updated `CategoryModel` object.
    ```json
    {
        "id": 1,
        "userId": 15,
        "name": "Subscriptions & Dues",
        "createdAtUtc": "2024-05-01T10:00:00Z",
        "updatedAtUtc": "2024-05-24T18:30:00Z"
    }
    ```

#### Error Responses
*   **Status Code:** `404 Not Found`
*   **Status Code:** `500 Internal Server Error`

---

### 5. Delete a Category

Deletes an existing category.

*   **Endpoint:** `DELETE /Categories/{categoryId}`
*   **Description:** Permanently deletes the category with the specified `categoryId`. This action cannot be undone.
*   **Authorization:** Required (`User` or `Admin` role).

#### Success Response
*   **Status Code:** `204 No Content`
*   **Description:** No content is returned in the response body when the deletion is successful.

#### Error Responses
*   **Status Code:** `404 Not Found`
*   **Status Code:** `500 Internal Server Error`