# **FinTrack API: Secure Debt System (Debt Controller)**

This document describes the `DebtController` endpoints that manage FinTrack's innovative **Secure Debt System (GBS)**. The GBS provides a secure environment for users to lend and borrow money with video verification, intended to serve as legally admissible evidence.

*Controller Base Path:* `/Debt`

---

## General Information

### Authentication

**All endpoints** in this controller require authorization. Requests must include a valid JWT `Bearer Token` in the `Authorization` header.

### Secure Debt System (GBS) Workflow

1.  **Create Offer (`/create-debt-offer`):** The "Lender" creates a debt offer using the "Borrower's" email address.
2.  **Respond to Offer (`/respond-to-offer/{debtId}`):** The "Borrower" accepts or rejects the incoming offer.
    *   **If Accepted:** The debt's status is updated to `AcceptedPendingVideoUpload`.
    *   **If Rejected:** The debt's status is updated to `RejectedByBorrower`, and the process ends.
3.  **Video Upload (Separate Service):** At this stage, the borrower uploads the security video to the system. This process is managed by a separate video management service.
4.  **Operator Approval (Admin Panel):** An operator reviews the uploaded video and debt details. If approved, the debt's status becomes `Active`.
5.  **Mark as Defaulted (`/mark-as-defaulted/{debtId}`):** If the debt is not paid by its due date, the "Lender" can mark the debt as `Defaulted`. This action activates the lender's right to access the video evidence.

### `status` Field Values (`DebtStatusType`)

In API requests and responses, the `status` field can take the following string values:
*   `PendingBorrowerAcceptance`
*   `AcceptedPendingVideoUpload`
*   `PendingOperatorApproval`
*   `Active`
*   `Paid`
*   `RejectedByBorrower`
*   `RejectedByOperator`
*   `Defaulted`
*   `Cancelled`

---

## Endpoints

### 1. List a User's Debts

Lists all debt records where the logged-in user is either the "lender" or the "borrower".

*   **Endpoint:** `GET /Debt`
*   **Description:** Returns a detailed list of all debts in which the user is involved.
*   **Authorization:** Required.

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** An array of `DebtDto` objects.
    ```json
    [
        {
          "id": 1,
          "lenderId": 15,
          "lenderName": "Ali_Veli",
          "borrowerId": 22,
          "borrowerName": "Ayse_Yilmaz",
          "amount": 2500,
          "currency": "TRY",
          "dueDateUtc": "2024-07-15T00:00:00Z",
          "description": "For an urgent need",
          "status": "Active",
          "videoMetadataId": "guid-for-video...",
          // ... other date and user detail fields
        }
    ]
    ```

---

### 2. Get Details of a Single Debt

Retrieves all details of a single debt specified by its ID.

*   **Endpoint:** `GET /Debt/{Id}`
*   **Description:** Returns the details of the debt with the given `Id`.
*   **Authorization:** Required (The user must be one of the parties to the debt).

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** A single `DebtDto` object.
    ```json
    {
      "id": 1,
      "lenderId": 15,
      "lenderName": "Ali_Veli",
      "borrowerId": 22,
      "borrowerName": "Ayse_Yilmaz",
      "amount": 2500,
      "currency": "TRY",
      "dueDateUtc": "2024-07-15T00:00:00Z",
      "description": "For an urgent need",
      "status": "Active",
      "videoMetadataId": "guid-for-video...",
      // ... other date and user detail fields
    }
    ```

#### Error Responses
*   **Status Code:** `404 Not Found`

---

### 3. Create a Debt Offer (Step 1)

Allows a lender to send a new debt offer to a borrower.

*   **Endpoint:** `POST /Debt/create-debt-offer`
*   **Description:** Creates a new debt record and sets its status to `PendingBorrowerAcceptance`.
*   **Authorization:** Required.

#### Request Body (`CreateDebtOfferRequestDto`)
| Field | Type | Description | Required? |
| :--- | :--- | :--- | :--- |
| `borrowerEmail` | `string` | The email address of the user to whom the debt is being offered. | Yes |
| `amount` | `number` | The amount of the debt. | Yes |
| `currencyCode` | `string` | The currency (e.g., "TRY", "USD"). | Yes |
| `dueDateUtc` | `string` | The due date for repayment (ISO 8601). | Yes |
| `description` | `string` | A short description of the debt. | No |

#### Success Response
*   **Status Code:** `200 OK`
    ```json
    {
      "success": true,
      "message": "Debt offer created successfully. Waiting for borrower's approval.",
      "debtId": 12
    }
    ```

---

### 4. Respond to a Debt Offer (Step 2)

Allows a borrower to accept or reject a debt offer they have received.

*   **Endpoint:** `POST /Debt/respond-to-offer/{debtId}`
*   **Description:** Only the user who is the borrower can call this endpoint. It updates the status of the debt.
*   **Authorization:** Required (Only the borrower).

#### Request Body (`RespondToOfferRequestDto`)
| Field | Type | Description | Required? |
| :--- | :--- | :--- | :--- |
| `accepted` | `boolean` | If `true`, the offer is accepted; if `false`, it is rejected. | Yes |

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** `true`

#### Error Responses
*   **Status Code:** `403 Forbidden`: If the user attempting the action is not the borrower of the debt.
*   **Status Code:** `400 Bad Request`: If the offer is not in a state that can be responded to (e.g., its `status` is not `PendingBorrowerAcceptance`).

---

### 5. Mark Debt as Defaulted (Step 5)

Allows a lender to mark an overdue debt as "Defaulted".

*   **Endpoint:** `POST /Debt/mark-as-defaulted/{debtId}`
*   **Description:** Only the user who is the lender can call this endpoint. The debt must be past its due date and have a status of `Active`.
*   **Authorization:** Required (Only the lender).

#### Success Response
*   **Status Code:** `200 OK`
    ```json
    {
      "success": true,
      "message": "Debt has been successfully marked as defaulted. You can now access the video evidence."
    }
    ```

#### Error Responses
*   **Status Code:** `403 Forbidden`: If the user attempting the action is not the lender of the debt.
*   **Status Code:** `400 Bad Request`: If the debt is not yet overdue or its `status` is not `Active`.