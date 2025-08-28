# **FinTrack API: Membership and Plan Management (Membership Controller)**

This document describes the `MembershipController` endpoints, which manage FinTrack's membership plans, user memberships, and payment processes via Stripe. This controller includes functionalities for both end-users and administrators.

*Controller Base Path:* `/Membership`

---

## Endpoint Categories

The endpoints in this controller are divided into three main groups:

1.  **Subscription Plans:** These are public (`AllowAnonymous`) endpoints that allow users and potential customers to view available membership plans and their features.
2.  **Admin Functions (Admin-Only):** These endpoints are restricted to users with the `Admin` role and are used to manage membership plans (create, update, delete).
3.  **User Membership Management (User-Specific):** These endpoints are for logged-in users to view their current membership, review their membership history, subscribe to a new plan (`create-checkout-session`), or cancel their existing subscription.

---

## 1. Subscription Plans (Public)

### 1.1. Get All Active Subscription Plans

Lists all active and purchasable membership plans in the system.

*   **Endpoint:** `GET /Membership/plans`
*   **Authorization:** Not required (`AllowAnonymous`).

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:** An array of `PlanFeatureDto` objects.
    ```json
    [
        {
            "id": 1,
            "name": "Free",
            "description": "For basic and entry-level use.",
            "price": 0,
            "currency": "USD",
            "billingCycle": "Monthly",
            // ... other plan features
        },
        {
            "id": 2,
            "name": "Plus",
            "description": "For intermediate and advanced users.",
            "price": 10,
            "currency": "USD",
            // ... other plan features
        }
    ]
    ```

### 1.2. Get a Specific Subscription Plan

Retrieves the details of a single active subscription plan specified by its ID.

*   **Endpoint:** `GET /Membership/plan/{Id}`
*   **Authorization:** Not required (`AllowAnonymous`).

---

## 2. Admin Functions (Admin Role Required)

### 2.1. Create a New Subscription Plan

Adds a new membership plan to the system.

*   **Endpoint:** `POST /Membership/plan`
*   **Authorization:** Required (`Admin` role).

#### Request Body (`PlanFeatureCreateDto`)
| Field | Type | Description |
| :--- | :--- | :--- |
| `planName` | `string` | The name of the plan (e.g., "Pro"). |
| `price` | `number` | The price of the plan. |
| `currency` | `string` | The currency (e.g., "USD"). |
| `billingCycle`|`string`| The billing cycle (e.g., "Monthly", "Yearly").|
| `isActive` | `boolean`| Whether the plan is available for purchase. |
| ... | ... | Other features (`reporting`, `budgeting`, etc.) |

#### Success Response
*   A `201 Created` status code and the created plan object.

### 2.2. Update a Subscription Plan

Updates the details of an existing membership plan.

*   **Endpoint:** `PUT /Membership/plan/{Id}`
*   **Authorization:** Required (`Admin` role).

### 2.3. Delete a Subscription Plan

Removes an existing membership plan from the system.

*   **Endpoint:** `DELETE /Membership/plan/{Id}`
*   **Authorization:** Required (`Admin` role).

---

## 3. User Membership Management (Logged-In User)

### 3.1. Get Current Membership

Retrieves the logged-in user's current and active membership.

*   **Endpoint:** `GET /Membership/current`
*   **Authorization:** Required (`User` or `Admin` role).

### 3.2. Get Membership History

Lists all past and current memberships for the user.

*   **Endpoint:** `GET /Membership/history`
*   **Authorization:** Required (`User` or `Admin` role).

### 3.3. Create Checkout Session (Stripe Integration)

Creates a session to initiate the Stripe payment page for a user to subscribe to a selected plan.

*   **Endpoint:** `POST /Membership/create-checkout-session`
*   **Description:** This endpoint takes a plan ID, creates a membership record in the database with a `PendingPayment` status, and connects to the Stripe API to start a payment session. It returns the URL of the Stripe payment page to which the user should be redirected.
*   **Authorization:** Required (`User` or `Admin` role).

#### Request Body (`SubscriptionRequestDto`)
| Field | Type | Description |
| :--- | :--- | :--- |
| `planId` | `integer` | The ID of the plan to subscribe to. |
| `autoRenew`| `boolean`| Whether the subscription should auto-renew. |

#### Success Response
*   **Status Code:** `200 OK`
*   **Content:**
    ```json
    {
      "sessionId": "cs_test_a1B2c3D4...",
      "checkoutUrl": "https://checkout.stripe.com/c/pay/cs_test_a1B2c3D4..."
    }
    ```

### 3.4. Cancel a Subscription

Stops the automatic renewal of a user's active subscription.

*   **Endpoint:** `POST /Membership/{userMembershipId}/cancel`
*   **Description:** Updates the subscription's status to `Cancelled`. The membership remains active until the end of the current billing period.
*   **Authorization:** Required (`User` or `Admin` role).