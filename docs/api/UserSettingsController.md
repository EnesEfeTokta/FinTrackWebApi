# **FinTrack API: User Settings Management (UserSettings Controller)**

This document describes the `UserSettingsController` endpoints, which allow users to manage their own profile information, account security, application preferences, and notification settings.

*Controller Base Path:* `/UserSettings`

---

## General Information

### Authentication

**All endpoints** in this controller require authorization. Requests must include a valid JWT `Bearer Token` in the `Authorization` header.

---

## 1. Account Security and Identity Management

### 1.1. Update Username

Changes the user's `UserName` in the system by updating their first and last name.

*   **Endpoint:** `POST /UserSettings/update-username`
*   **Description:** Creates a new username by combining the given first and last name (e.g., `John_Doe`).

#### Request Body (`UpdateUserNameDto`)
| Field | Type | Description |
| :--- | :--- | :--- |
| `firstName` | `string` | The user's new first name. |
| `lastName` | `string` | The user's new last name. |

#### Success Response
*   `200 OK` with `{"message": "Username updated successfully.", "newUserName": "..."}`.

#### Error Responses
*   `409 Conflict`: If the new username is already taken by another user.

### 1.2. Request Email Change (Step 1/2)

Initiates a **secure process** to change the user's email address.

*   **Endpoint:** `POST /UserSettings/request-email-change`
*   **Description:** To verify the user's identity, an OTP code, valid for 15 minutes, is sent to their **current email address**. This prevents the email from being changed without the account owner's consent.

#### Success Response
*   `200 OK` with `{"message": "An OTP has been sent to your current email address to verify your identity."}`.

### 1.3. Confirm Email Change (Step 2/2)

Permanently changes the email address of the user who has verified their identity with the OTP.

*   **Endpoint:** `POST /UserSettings/confirm-email-change`
*   **Description:** If the OTP is verified, the user's email is updated to the new address.

#### Request Body (`UpdateUserEmailDto`)
| Field | Type | Description |
| :--- | :--- | :--- |
| `newEmail` | `string` | The user's new email address. |
| `otpCode` | `string` | The OTP code received at the current email address. |

#### Error Responses
*   `400 Bad Request`: If the OTP is incorrect or has expired.
*   `409 Conflict`: If the new email address is already in use by another user.

### 1.4. Update Password

Allows the user to set a new password by verifying their current one.

*   **Endpoint:** `POST /UserSettings/update-password`

#### Request Body (`UpdateUserPasswordDto`)
| Field | Type | Description |
| :--- | :--- | :--- |
| `currentPassword`| `string` | The user's current password. |
| `newPassword` | `string` | The user's new password. |

#### Error Responses
*   `400 Bad Request`: If the current password is incorrect.

---

## 2. Profile and Application Settings

### 2.1. Update Profile Picture

Updates the URL of the user's profile picture.

*   **Endpoint:** `POST /UserSettings/update-profile-picture`

#### Request Body (`UpdateProfilePictureDto`)
| Field | Type | Description |
| :--- | :--- | :--- |
| `profilePictureUrl`| `string` | The full URL of the new profile picture. |

### 2.2. Get Application Settings

Retrieves the user's application-wide settings, such as theme, language, and default currency.

*   **Endpoint:** `GET /UserSettings/app-settings`

### 2.3. Update Application Settings

Updates the user's application-wide settings.

*   **Endpoint:** `POST /UserSettings/app-settings`

#### Request Body (`UserAppSettingsUpdateDto`)
| Field | Type | Description |
| :--- | :--- | :--- |
| `appearance` | `string` | Theme (`Light`, `Dark`). |
| `currency` | `string` | Default currency (`TRY`, `USD`, `EUR`). |
| `language` | `string` | Application language (`tr_TR`, `en_US`). |

---

## 3. Notification Preferences

### 3.1. Get Notification Settings

Retrieves the settings that specify which types of notifications the user wishes to receive.

*   **Endpoint:** `GET /UserSettings/user-notification-settings`

### 3.2. Update Notification Settings

Updates the user's notification preferences.

*   **Endpoint:** `POST /UserSettings/user-notification-settings`

#### Request Body (`UserNotificationSettingsUpdateDto`)
| Field | Type | Description |
| :--- | :--- | :--- |
| `spendingLimitWarning`| `boolean` | Budget limit warning. |
| `expectedBillReminder`| `boolean` | Expected bill reminder. |
| `weeklySpendingSummary`| `boolean`| Weekly spending summary. |
| `newFeaturesAndAnnouncements`| `boolean` | New features and announcements. |
| `enableDesktopNotifications`| `boolean` | Enable desktop notifications. |

---

## 4. Dashboard Preferences

### 4.1. Get Dashboard Preferences

Retrieves the user's personal dashboard preferences.

*   **Endpoint:** `GET /UserSettings/user-dashboard`

### 4.2. Update Dashboard Preferences

Updates the user's dashboard preferences.

*   **Endpoint:** `POST /UserSettings/user-dashboard`

#### Request Body (`UserDashboardSettingsUpdateDto`)
| Field | Type | Description |
| :--- | :--- | :--- |
| `selectedCurrencies`| `integer[]` | Selected currencies. Max 5 items. |
| `selectedBudgets`| `integer[]` | Selected budgets. Max 4 items. |
| `selectedAccounts`| `integer[]`| Selected accounts. Max 2 items. |