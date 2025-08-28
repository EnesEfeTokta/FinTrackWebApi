# **FinTrack API: User Authentication (UserAuth Controller)**

This document describes the `UserAuthController` endpoints used for user registration, identity verification, and login.

*Controller Base Path:* `/UserAuth`

---

## General Information

### Authentication

The endpoints in this controller are public and do not require an `Authorization` header. Authentication processes are performed through these endpoints.

### Workflow: Two-Step User Registration

FinTrack uses a two-step registration process to ensure security:

1.  **Step 1 (Initiate):** The user submits their basic information to the system via the `initiate-registration` endpoint. The system temporarily stores this information and a one-time password (OTP) in the database and sends the OTP to the user's email address.
2.  **Step 2 (Verify & Register):** The user submits the OTP received via email to the system using the `verify-otp-and-register` endpoint. If the OTP is verified, the system creates a permanent user record using the temporary data, defines default settings and a membership, and then deletes the temporary data.

This approach both verifies the ownership of the email address and prevents invalid registrations from burdening the system.

---

## Endpoints

### 1. Initiate Registration and Send OTP

Initiates the first step of a new user registration.

*   **Endpoint:** `POST /UserAuth/initiate-registration`
*   **Description:** Validates the information received from the user, creates an OTP valid for 5 minutes, temporarily stores the information, and sends a verification email containing the OTP.
*   **Authorization:** Not required (Public).

#### Request Body (`UserInitiateRegistrationDto`)

| Field | Type | Description | Required? |
| :--- | :--- | :--- | :--- |
| `email` | `string` | The user's valid email address. | Yes |
| `firstName` | `string` | The user's first name. | Yes |
| `lastName` | `string` | The user's last name. | Yes |
| `password` | `string` | A strong password chosen by the user. | Yes |
| `profilePicture`| `string` | The URL of the profile picture. | No |

#### Request Body Example
```json
{
  "email": "sample.user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "password": "Password123!",
  "profilePicture": "https://example.com/path/to/image.jpg"
}
```

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:**
    ```json
    {
      "message": "OTP has been sent to your email address. Please verify to complete registration."
    }
    ```

#### Error Responses
*   **Status Code:** `400 Bad Request`
    *   When required information is missing: `{"message": "Email, Username, and Password are required."}`
    *   If the email address is already registered: `{"message": "This email address is already registered."}`
    *   If the username (FirstName_LastName) is already taken: `{"message": "This username is already taken."}`
*   **Status Code:** `500 Internal Server Error`
    *   If the OTP cannot be saved to the database or an error occurs during email dispatch.

---

### 2. Verify OTP and Complete Registration

Performs the second and final step of the registration process.

*   **Endpoint:** `POST /UserAuth/verify-otp-and-register`
*   **Description:** Verifies the OTP submitted by the user. If successful, it creates the permanent user record, assigns default roles, settings, and a membership, and sends a welcome email.
*   **Authorization:** Not required (Public).

#### Request Body (`VerifyOtpRequestDto`)
| Field | Type | Description | Required? |
| :--- | :--- | :--- | :--- |
| `email` | `string` | The email address to which the OTP was sent. | Yes |
| `code` | `string` | The 6-digit OTP code from the email. | Yes |

#### Request Body Example
```json
{
  "email": "sample.user@example.com",
  "code": "123456"
}
```

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:**
    ```json
    {
      "message": "User registration successful. You can now log in.",
      "userId": 123
    }
    ```

#### Error Responses
*   **Status Code:** `400 Bad Request`
    *   If the OTP code is incorrect or has expired: `{"message": "Invalid or expired OTP code."}`
    *   If the user cannot be created due to a reason like an ASP.NET Identity password policy violation: `{"message": "User registration failed.", "errors": ["Passwords must be at least 6 characters.", "Passwords must have at least one non-alphanumeric character." ... ]}`
*   **Status Code:** `500 Internal Server Error`
    *   If an error occurs while creating default settings for the user.

---

### 3. User Login and Get Tokens

Allows a registered user to log in to the system and receive session tokens.

*   **Endpoint:** `POST /UserAuth/login`
*   **Description:** Authenticates the user with their email and password. If successful, it generates an `AccessToken` and `RefreshToken` to be used for accessing the API.
*   **Authorization:** Not required (Public).

#### Request Body (`LoginDto`)
| Field | Type | Description | Required? |
| :--- | :--- | :--- | :--- |
| `email` | `string` | The user's registered email address. | Yes |
| `password` | `string` | The user's password. | Yes |

#### Request Body Example
```json
{
  "email": "sample.user@example.com",
  "password": "Password123!"
}
```

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:**
    ```json
    {
      "userId": 123,
      "userName": "John_Doe",
      "email": "sample.user@example.com",
      "profilePicture": "https://.../image.jpg",
      "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "refreshToken": "another_long_secure_random_string...",
      "roles": ["User"]
    }
    ```

#### Error Responses
*   **Status Code:** `401 Unauthorized`
    *   If the email or password is incorrect: `{"message": "Invalid credentials."}`
    *   If the account is locked out due to too many failed login attempts: `{"message": "Account locked out. Please try again later. (Until: ...)", "isLockedOut": true, "lockoutEndDateUtc": "..."}`