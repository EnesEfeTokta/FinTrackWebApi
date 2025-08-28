# **FinTrack API: Stripe Webhook Receiver**

This document describes the `StripeWebhookController` endpoint, which listens for real-time events from Stripe and automates payment processes.

*Controller Base Path:* `/api/stripe/webhook`

---

## General Information

### Authentication and Security

*   **Endpoint:** This endpoint is **public** (`AllowAnonymous`) because the Stripe service needs to be able to send requests to it without authentication.
*   **Webhook Security (Signature Verification):** Although the endpoint is public, a security mechanism is used to verify that every incoming request genuinely originates from Stripe. Stripe includes a special HTTP header called `Stripe-Signature` in every request. The controller verifies the legitimacy of the request by comparing this signature with a secret key (`WebhookSecret`) stored in the `appsettings.json` file. **If the signature is invalid, the request is rejected.** This prevents fraudulent payment notifications.

### Role in the Architecture: Event-Driven Automation

This controller is not called directly by users. It functions as an **event listener**.

1.  **Payment Success:** A user successfully completes a payment on the Stripe checkout page initiated via the `MembershipController`.
2.  **Stripe Sends an Event:** Stripe notifies this pre-configured webhook endpoint of the successful payment event (`checkout.session.completed`) by sending a `POST` request.
3.  **Webhook Processing:** The `StripeWebhookController` receives this request and automatically performs the following actions:
    *   Verifies the request's signature.
    *   Parses the data within the event (payment ID, membership ID, etc.).
    *   Updates the relevant user's membership status in the database from `PendingPayment` to `Active`.
    *   Marks the payment record as `Succeeded`.
    *   Sends a confirmation email to the user with successful payment and invoice details.

This architecture manages the payment and membership activation process securely and automatically, without human intervention.

---

## Endpoints

### 1. Handle Stripe Webhook Events

The single endpoint that accepts and processes all webhook events sent by Stripe.

*   **Endpoint:** `POST /api/stripe/webhook`
*   **Description:** This endpoint actively processes only the `checkout.session.completed` event. Other event types are currently logged but do not trigger any action.
*   **Authorization:** Not required (`AllowAnonymous`). Security is ensured via signature verification.

#### Request Body

*   **Content-Type:** `application/json`
*   **Content:** The body of this request is generated directly by Stripe and has the structure of a `Stripe.Event` object. It does not need to be manually created.

#### Success Response

*   **Status Code:** `200 OK`
*   **Description:** This endpoint **always** returns `200 OK` (except in cases of signature errors or critical server failures) to signal to Stripe that "the event was received and processed successfully." This prevents Stripe from repeatedly sending the same event.
*   **Content:** The response body is empty.

#### Error Responses

*   `400 Bad Request`:
    *   If the `Stripe-Signature` header is invalid or missing.
    *   If the data (metadata) within the Stripe event is missing or incorrect.
*   `404 Not Found`:
    *   If the `UserMembershipId` or `PaymentId` specified in the Stripe event's metadata cannot be found in the database.
*   `500 Internal Server Error`:
    *   If an unexpected error occurs during database operations or email dispatch.

Even if these errors occur, the endpoint will try to return `200 OK` to Stripe and log the error. In critical situations (like a signature mismatch), it will return a `4xx` status.