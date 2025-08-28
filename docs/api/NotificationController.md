# **FinTrack API: Notification Management (Notification Controller)**

This document describes the `NotificationController` endpoints used for creating, viewing, and managing in-app notifications for users.

*Controller Base Path:* `/Notification`

---

## General Information

### Authentication

**All endpoints** in this controller require authorization. Requests must include a valid JWT `Bearer Token` in the `Authorization` header. All operations are performed within the scope of the token's owner.

### `notificationType` Field Values (`NotificationType`)

Notifications are classified based on their content. The `notificationType` field can take the following string values:
*   `Info`
*   `Warning`
*   `Error`
*   `Success`
*   `System` (System Message)
*   `Debt` (Debt Notification)

---

## Endpoints

### 1. Get All of a User's Notifications

Lists all notifications for the logged-in user, sorted from newest to oldest.

*   **Endpoint:** `GET /Notification`
*   **Description:** Returns a list of all notifications belonging to the authenticated user.
*   **Authorization:** Required (`User` or `Admin` role).

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** An array of `NotificationDto` objects.
    ```json
    [
        {
          "id": 1,
          "messageHead": "New Debt Offer",
          "messageBody": "You have received a new debt offer of 500 TRY from the user Ali Veli.",
          "notificationType": "Debt",
          "createdAt": "2024-05-24T10:00:00Z",
          "isRead": false
        },
        {
          "id": 2,
          "messageHead": "Budget Alert",
          "messageBody": "You have reached 80% of your 'Groceries' budget.",
          "notificationType": "Warning",
          "createdAt": "2024-05-23T15:30:00Z",
          "isRead": true
        }
    ]
    ```

#### Error Responses
*   `500 Internal Server Error`

---

### 2. Mark a Single Notification as Read

Marks a single notification with the specified ID as "read".

*   **Endpoint:** `POST /Notification/mark-as-read/{id}`
*   **Description:** If the notification is already read, no action is taken.
*   **Authorization:** Required (`User` or `Admin` role).

#### Success Response
*   **Status Code:** `204 No Content`
*   **Description:** No content is returned in the response body when the operation is successful.

#### Error Responses
*   `404 Not Found`: If the notification is not found or does not belong to the user.
*   `500 Internal Server Error`

---

### 3. Mark All Notifications as Read

Marks all of the user's unread notifications as "read" in a single operation.

*   **Endpoint:** `POST /Notification/mark-all-as-read`
*   **Description:** Performs a bulk update operation in the database.
*   **Authorization:** Required (`User` or `Admin` role).

#### Success Response
*   **Status Code:** `204 No Content`

---

### 4. Create a New Notification (Generally Used by the System)

Creates a new notification for a user. This endpoint is typically triggered by other events in the system (e.g., when a new debt offer is received) rather than being called directly by the user.

*   **Endpoint:** `POST /Notification`
*   **Authorization:** Required (`User` or `Admin` role).

#### Request Body (`NotificationCreateDto`)
| Field | Type | Description | Required? |
| :--- | :--- | :--- | :--- |
| `messageHead`| `string` | The title of the notification. | Yes |
| `messageBody`| `string` | The detailed content of the notification. | Yes |
|`notificationType`|`string`| The type of notification (See `notificationType` fields).| Yes |

#### Success Response
*   A `201 Created` status code and the created notification as a `NotificationDto` object.

---

### 5. Delete a Single Notification

Permanently deletes a single notification with the specified ID.

*   **Endpoint:** `DELETE /Notification/{id}`
*   **Authorization:** Required (`User` or `Admin` role).

#### Success Response
*   **Status Code:** `204 No Content`

#### Error Responses
*   `404 Not Found`: If the notification is not found or does not belong to the user.

---

### 6. Clear All Notifications

Permanently deletes all of a user's notifications (read or unread) in a single operation.

*   **Endpoint:** `DELETE /Notification/clear-all`
*   **Description:** Performs a bulk delete operation in the database.
*   **Authorization:** Required (`User` or `Admin` role).

#### Success Response
*   **Status Code:** `204 No Content`