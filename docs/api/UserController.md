# **FinTrack API: User Profile and Data Hub (User Controller)**

This document describes the `UserController` endpoint, which provides a centralized source for all of the logged-in user's personal information, settings, membership status, and a summary of their financial data.

*Controller Base Path:* `/User`

---

## General Information

### Authentication

**All endpoints** in this controller require authorization. Requests must include a valid JWT `Bearer Token` in the `Authorization` header.

### Role in the Architecture: Data Aggregator

This controller is designed to serve as a **data aggregator**, providing all essential data that a client application (WPF, Mobile, etc.) might need upon user session start **in a single API call**. This approach:
*   Reduces the number of initial API calls the client needs to make.
*   Improves application startup performance.
*   Provides a consistent snapshot of all user-related data.

In the background, the controller joins data from multiple database tables (`Users`, `UserMemberships`, `Accounts`, `Budgets`, etc.) to create a rich `UserProfileDto` object.

---

## Endpoints

### 1. Get All Information for the Logged-In User

Returns a comprehensive data package containing the token owner's profile information, settings, active membership, and ID lists of all their financial assets.

*   **Endpoint:** `GET /User`
*   **Description:** This endpoint is generally called immediately after the user logs into the application.
*   **Authorization:** Required (`User` role).

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** A `UserProfileDto` object.
    ```json
    {
      // --- Basic Information ---
      "id": 15,
      "userName": "Ahmet_Yilmaz",
      "email": "ahmet.yilmaz@example.com",
      "profilePictureUrl": "https://.../image.jpg",
      "createdAtUtc": "2024-01-10T14:00:00Z",

      // --- Membership Information ---
      "currentMembershipPlanId": 2,
      "currentMembershipPlanType": "Plus",
      "membershipStartDateUtc": "2024-05-20T10:00:00Z",
      "membershipExpirationDateUtc": "2025-05-20T10:00:00Z",

      // --- User Settings ---
      "thema": "Light",
      "language": "en_US",
      "currency": "TRY",
      "spendingLimitWarning": true,
      "expectedBillReminder": true,
      "weeklySpendingSummary": false,
      "newFeaturesAndAnnouncements": true,
      "enableDesktopNotifications": true,

      // --- Usage Data (ID Lists) ---
      "currentAccounts": [1, 2, 5],
      "currentBudgets": [10, 11],
      "currentTransactions": [101, 102, 103, 104],
      "currentBudgetsCategories": [20, 21],
      "currentTransactionsCategories": [30, 31, 32],
      "currentLenderDebts": [5],
      "currentBorrowerDebts": [6, 7],
      "currentNotifications": [201, 202],
      "currentFeedbacks": [51],
      "currentVideos": [1]
    }
    ```

#### Error Responses
*   `401 Unauthorized`: When a valid token is not provided.
*   `500 Internal Server Error`: If an unexpected server error occurs while aggregating the data.