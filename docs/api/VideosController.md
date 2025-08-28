# **FinTrack API: GBS Video Management (Videos Controller)**

This document describes the `VideosController` endpoints, which manage the **video evidence mechanism** that forms the foundation of the Secure Debt System (GBS). This controller is responsible for the processes of video uploading, operator approval, encryption, and secure video streaming.

*Controller Base Path:* `/Videos`

---

## General Information

### Authentication

**All endpoints** in this controller require authorization. Each endpoint has its own specific role-based and ownership-based authorization rules.

### Cryptography and Security Model

1.  **Temporary Storage:** Uploaded videos are initially stored unencrypted in a temporary location on the server.
2.  **Operator Approval:** When an operator approves a video, the system generates a random and strong **20-character encryption key** (`userPasswordKey`).
3.  **AES Encryption:** The video is encrypted using this key with the **AES** algorithm and moved to a secure location. The original (unencrypted) file is permanently deleted.
4.  **Key Delivery:** The generated **20-character key** is delivered to the debt's lender via email. This key is **not stored** in the system; only a hashed version is kept for verification purposes. The security of the key is entirely the lender's responsibility.
5.  **Secure Streaming:** When a debt is in default, the lender can use their key to decrypt and watch the video.

### `VideoStatusType` Values
*   `PendingApproval`
*   `ProcessingEncryption`
*   `Encrypted`
*   `Rejected`
*   `ProcessingError`
*   `EncryptionFailed`

---

## Endpoints

### 1. Video Upload by Borrower (Step 3)

Allows the borrower to upload a commitment video for a debt offer they have accepted.

*   **Endpoint:** `POST /Videos/user-upload-video`
*   **Description:** This endpoint accepts a video file in `multipart/form-data` format. It temporarily stores the video on the server and updates the debt's status to `PendingOperatorApproval`.
*   **Authorization:** Required. Only the "Borrower" of the debt can perform this action.
*   **Request Type:** `multipart/form-data`

#### Form Data
| Field | Type | Description | Required? |
| :--- | :--- | :--- | :--- |
| `file` | `File` | The user's commitment video. | Yes |
| `debtId` | `integer`| The ID of the debt associated with the video. | Yes |

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** The video's metadata.
    ```json
    {
      "message": "Video metadata saved successfully.",
      "videoMetadata": {
        "id": 1,
        "uploadedByUserId": 22,
        "originalFileName": "commitment.mp4",
        "fileSize": 15728640,
        "contentType": "video/mp4",
        "status": "PendingApproval"
        // ... other metadata fields
      }
    }
    ```

#### Error Responses
*   `403 Forbidden`: If the user performing the action is not the borrower of the debt.
*   `400 Bad Request`: If the debt is not in a status that allows for video upload (i.e., not `AcceptedPendingVideoUpload`).

---

### 2. Approve and Encrypt Video (Step 4)

Allows an operator to approve an uploaded video and trigger the encryption process.

*   **Endpoint:** `POST /Videos/video-approve/{videoId}`
*   **Description:** When called by an operator, this endpoint encrypts the video, deletes the original file, and sets the debt's status to `Active`. It then sends an email containing the encryption key to the lender.
*   **Authorization:** Required. This action can only be performed by users with the `VideoApproval` or `Admin` role.

#### Success Response
*   **Status Code:** `200 OK`
    ```json
    {
      "message": "Video was successfully approved and encrypted.",
      "videoMeta": {
        "id": 1,
        "status": "Encrypted",
        "storageType": "EncryptedFileSystem"
        // ... other metadata fields
      }
    }
    ```

#### Error Responses
*   `404 Not Found`: If the video or the associated debt cannot be found.
*   `400 Bad Request`: If the video has already been processed.
*   `500 Internal Server Error`: If an error occurs during encryption or email dispatch.

---

### 3. Stream/Watch Encrypted Video (Step 6)

Allows the lender of a defaulted debt to watch the video using their key.

*   **Endpoint:** `GET /Videos/video-metadata-stream/{videoId}`
*   **Description:** This endpoint decrypts the encrypted video file on-the-fly using the key provided as a query parameter and sends it to the client as a file stream.
*   **Authorization:** Required. Only the "Lender" of the debt or users with the `Admin` role can perform this action, and only when the debt status is `Defaulted`.

#### URL Parameters
| Parameter | Type | Description | Required? |
| :--- | :--- | :--- | :--- |
| `videoId` | `integer`| The ID of the video metadata to be streamed. | Yes |

#### Query Parameters
| Parameter | Type | Description | Required? |
| :--- | :--- | :--- | :--- |
| `key` | `string` | The 20-character key sent to the lender via email to decrypt the video. | Yes |

#### Success Response
*   **Status Code:** `200 OK`
*   **Content-Type:** The original `Content-Type` of the video (e.g., `video/mp4`).
*   **Content:** The decrypted video file itself (binary stream).

#### Error Responses
*   `401 Unauthorized`: If the provided `key` is incorrect.
*   `403 Forbidden`: If the user is not the lender of the debt.
*   `400 Bad Request`: If the debt status is not `Defaulted`.