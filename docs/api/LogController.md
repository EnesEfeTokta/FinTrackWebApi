# **FinTrack API: System Log Management (Log Controller)**

This document describes the `LogController` endpoint, which facilitates debugging and system monitoring processes by providing access to log files generated on the server.

*Controller Base Path:* `/Log`

---

## General Information

### Authentication and Security Warning

*   **Authentication:** The endpoint in this controller is **public** and does not require any authentication or authorization.
*   **CAUTION:** The public nature of this endpoint can pose a significant security risk in production environments. Therefore, access to this endpoint must be protected by network-level security. For example:
    *   A **Firewall rule** should be defined to allow access only from specific IP addresses.
    *   It should only be accessible from an internal network or via a **VPN**.
    *   Access control should be enforced by placing it behind an **API Gateway**.

### Path Traversal Protection

This endpoint includes **path traversal protection** to prevent access to other files on the system (e.g., `appsettings.json`) by using expressions like `../` in the file name. If the requested file path attempts to navigate outside the expected `/logs` directory, the request will be rejected with a `400 Bad Request` error.

---

## Endpoints

### 1. Download a Specific Log File

Downloads a specific log file located in the server's `/logs` directory.

*   **Endpoint:** `GET /Log/{fileName}`
*   **Description:** Returns the content of the log file using the specified file name.
*   **Authorization:** Not required (`Public`). **Security must be handled at the network layer.**

#### URL Parameters
| Parameter | Type | Description | Required? |
| :--- | :--- | :--- | :--- |
| `fileName` | `string` | The full name and extension of the log file to be downloaded (e.g., `fintrack-log-20240525.json`). | Yes |

#### Success Response
*   **Status Code:** `200 OK`
*   **Content-Type:** `application/json` (Assuming logs are in JSON format).
*   **Content:** The raw content of the requested log file.

#### Error Responses
*   **Status Code:** `400 Bad Request`
    *   If a path traversal attack attempt is detected in the `fileName` parameter (e.g., using `../`).
*   **Status Code:** `404 Not Found`
    *   If a log file with the specified `fileName` cannot be found in the `/logs` directory.
*   **Status Code:** `500 Internal Server Error`
    *   If an unexpected server error occurs while reading or downloading the file.