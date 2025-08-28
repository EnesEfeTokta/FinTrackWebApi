# **FinTrack API: Feedback Management (Feedback Controller)**

This document describes the `FeedbackController` endpoints, which allow users to send feedback about the application and view their past submissions.

*Controller Base Path:* `/Feedback`

---

## General Information

### Authentication

**All endpoints** in this controller require authorization. Requests must include a valid JWT `Bearer Token` in the `Authorization` header.

### `type` Field Values (`FeedbackType`)

Users can classify their feedback. The `type` field can take the following string values:
*   `BugReport`
*   `FeatureRequest`
*   `GeneralFeedback`
*   `Question`
*   `Other`

---

## Endpoints

### 1. Retrieve All of a User's Feedbacks

Lists all feedback previously submitted by the logged-in user.

*   **Endpoint:** `GET /Feedback`
*   **Description:** Returns a list of all feedback belonging to the authenticated user.
*   **Authorization:** Required (`User` or `Admin` role).

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** An array of `FeedbackDto` objects.
    ```json
    [
        {
          "id": 1,
          "subject": "Error on Reporting Screen",
          "description": "The application freezes when I try to generate a report in Excel format.",
          "type": "BugReport",
          "savedFilePath": "/path/to/screenshot.png",
          "createdAtUtc": "2024-05-23T10:00:00Z",
          "updatedAtUtc": null
        },
        {
          "id": 2,
          "subject": "Cryptocurrency Support",
          "description": "It would be great to be able to add my cryptocurrency wallets to my accounts.",
          "type": "FeatureRequest",
          "savedFilePath": null,
          "createdAtUtc": "2024-05-24T11:30:00Z",
          "updatedAtUtc": null
        }
    ]
    ```

#### Error Responses
*   `404 Not Found`: If the user has no feedback.
*   `500 Internal Server Error`

---

### 2. Get a Specific Feedback

Retrieves the details of a single feedback item belonging to the user by its ID.

*   **Endpoint:** `GET /Feedback/{Id}`
*   **Description:** Returns the details of the feedback with the given `Id` that belongs to the user.
*   **Authorization:** Required (`User` or `Admin` role).

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** A single `FeedbackDto` object (with the same structure as the example above).

#### Error Responses
*   `404 Not Found`
*   `500 Internal Server Error`

---

### 3. Create New Feedback

Allows a user to submit new feedback.

*   **Endpoint:** `POST /Feedback`
*   **Description:** Creates a new feedback record based on the provided information.
*   **Authorization:** Required (`User` or `Admin` role).

#### Request Body (`FeedbackCreateDto`)
| Field | Type | Description | Required? |
| :--- | :--- | :--- | :--- |
| `subject` | `string` | The subject/title of the feedback. | Yes |
| `description` | `string` | The detailed description of the feedback. | Yes |
| `type` | `string` | The type of feedback (See `type` field values). | Yes |
| `savedFilePath` | `string` | If applicable, the server path to a screenshot or file related to the feedback. | No |

#### Request Body Example
```json
{
  "subject": "Application Performance",
  "description": "The application runs very smoothly in general, thank you!",
  "type": "GeneralFeedback",
  "savedFilePath": null
}
```

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:**
    ```json
    true
    ```

#### Error Responses
*   `400 Bad Request`: If the request body is empty or incomplete.
*   `500 Internal Server Error`