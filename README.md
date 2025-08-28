# **FinTrack Project: Comprehensive Technical and Functional Documentation**

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/EnesEfeTokta/FinTrackWebApi)
[![License](https://img.shields.io/badge/license-GPL-blue)](LICENSE)
[![.NET Version](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Python Version](https://img.shields.io/badge/Python-3.10-blue)](https://www.python.org/)
[![Docker](https://img.shields.io/badge/docker-ready-blue)](https://www.docker.com/)

**A Next-Generation Financial Management Platform Built with a Microservices Architecture**

---

[<img src="https://t.ctcdn.com.br/gbO3hsV5DRUS3MYFIL0-vgDJtYk=/640x360/smart/i533291.png" width="50%">](https://youtu.be/EMGObq-SWrA)

[<img src="https://t.ctcdn.com.br/gbO3hsV5DRUS3MYFIL0-vgDJtYk=/640x360/smart/i533291.png" width="50%">](https://youtu.be/ZkjOf-GKKD0)

---

### **Table of Contents**

1.  **Executive Summary**
2.  **Project Vision and Target Audience**
3.  **Core Features and Capabilities**
4.  **Subscription Models and Revenue Strategy**
5.  **System Architecture**
    *   5.1. General Architecture and Microservices
    *   5.2. Backend Architecture
    *   5.3. Frontend (WPF) Architecture
    *   5.4. Database Architecture and Strategy
6.  **Technologies Used**
7.  **Key Systems and Workflows**
    *   7.1. User Authentication and Registration (OTP & JWT)
    *   7.2. Secure Debt System (GBS)
    *   7.3. Multi-Format Reporting System
    *   7.4. Dynamic Exchange Rate Management
8.  **DevOps, Containerization, and Monitoring**
    *   8.1. Docker Architecture and Services
    *   8.2. Monitoring and Health Checks (Prometheus & Grafana)
    *   8.3. Data Backup and Security
9.  **Installation, Configuration, and Running**
    *   9.1. Prerequisites
    *   9.2. Quick Setup with Docker (Recommended)
    *   9.3. Configuration (`appsettings.json`)
    *   9.4. Environment File Setup (`.env` for Docker Compose & FinBot)
10. **API Usage and Testing**
11. **Conclusion and Future Vision**
12. **License and Contact**

---

### **1. Executive Summary**

FinTrack is a next-generation **Software as a Service (SaaS)** platform that empowers both individual and professional users to take full control of their financial lives. Built on a microservices architecture, the project consists of the core business logic service **FinTrackWebApi**, the AI operations service **FinBotWebApi**, and the system administration service **WinTrackManagerPanel** (in development). With its comprehensive feature set, multi-format professional reporting, AI-powered smart assistant, and multi-platform support, FinTrack eliminates complexity in financial management, offering users clarity, security, and efficiency.

### **2. Project Vision and Target Audience**

**Vision:** To create the world's most intuitive and powerful financial management tool, increasing financial literacy and empowering users of all levels to achieve their financial goals.

**Target Audience:**
*   **Entry-Level Users:** Individuals looking to track personal expenses and create a budget.
*   **Intermediate and Advanced Users:** Individuals and families managing multiple accounts, investments, and budgets who require detailed analysis.
*   **Professionals and Freelancers:** Professionals who need to meticulously manage cash flow, require detailed reporting, and need legally valid debt tracking.

### **3. Core Features and Capabilities**

*   **Comprehensive Currency Support:** Track and convert over 950 global currencies in real-time.
*   **Strategic Budget Management:** Create dynamic budgets, set spending limits, and track goals.
*   **Centralized Account Tracking:** Manage all bank, credit card, and investment accounts from a single dashboard.
*   **Smart Income/Expense Analysis:** Automatically categorize transactions and provide insights into spending habits.
*   **Secure Debt System (GBS):** A peer-to-peer debt platform with video verification, encryption, and legally admissible evidence.
*   **AI-Powered Financial Assistant (FinBot):** An intelligent assistant that offers personalized financial advice, answers questions, and provides proactive solutions.
*   **Detailed and Flexible Reporting:** Generate professional reports in **PDF, WORD, TEXT, XML, EXCEL, and MARKDOWN** formats.
*   **Comprehensive Admin Panel (WinTrackManagerPanel):** System management, user supervision, content moderation, and system health monitoring.
*   **Data Visualization:** Turn data into understandable insights with interactive charts using **LiveCharts2**.

### **4. Subscription Models and Revenue Strategy**

FinTrack uses a freemium model to cater to different user segments. The payment infrastructure is integrated with **Stripe**, providing globally recognized security standards.

| Plan Name | Price | Target Audience |
| :--- | :--- | :--- |
| **Free** | 0 USD/Month | Basic financial tracking for entry-level users. |
| **Plus** | 10 USD/Month | Intermediate users managing multiple accounts and budgets. |
| **Pro** | 25 USD/Month | Professionals, freelancers, and users who need advanced features like GBS. |

### **5. System Architecture**

#### **5.1. General Architecture and Microservices**
FinTrack is built on a **Microservices Architecture**, consisting of independent, scalable, and flexible units. This structure allows each service to use its own technology stack and be developed and deployed independently.

1.  **FinTrackWebApi (Main API Service):** The heart of the project. It houses all critical business logic, including user management, authentication, account/transaction/budget management, Stripe integration, and reporting.
2.  **FinBotWebApi (ChatBot Service):** Manages artificial intelligence operations. Developed with **Python & FastAPI** for integration with **Ollama** and the **Mistral 7B** model.
3.  **WinTrackManagerPanel (Admin Panel):** A service designed for system administrators, providing administrative functions such as user management, GBS approval processes, system monitoring, and content moderation.

#### **5.2. Server-Side (Backend) Architecture**
*   **FinTrackWebApi & WinTrackManagerPanel:** Developed on **ASP.NET Core 8.0** following **RESTful API** principles and the **Dependency Injection (DI)** design pattern.
*   **FinBotWebApi:** Developed with **Python** and the high-performance **FastAPI** framework.

#### **5.3. Client-Side (Frontend - WPF) Architecture**
*   **FinTrackForWindows (WPF):** A native application providing a rich desktop experience for Windows. It communicates with FinTrackWebApi via secure **REST API** calls.
*   **Centralized Data Management (Store Pattern):** Data management is centralized using services (Stores) like `AccountStore` and `BudgetStore`. This ensures data consistency across components and optimizes API calls.

#### **5.4. Database Architecture and Strategy**

##### **Database**
*   **Database Engine:** PostgreSQL 15
*   **ORM (Object-Relational Mapper):** Entity Framework Core 8
*   **Approach:** Code-First with Migrations

##### **Database Architecture**

FinTrack uses a dual-database strategy to ensure data integrity and auditability:

1.  **MainDB:** The primary database that stores main application data (users, accounts, transactions, budgets, etc.).
2.  **LogDB:** A secondary database that records all data manipulation operations (POST, PUT, DELETE) on `MainDB` for auditing purposes.

##### **Detailed Database Schema (ERD & Table Structure)**

For comprehensive technical documentation covering all database tables, columns, data types, relationships (including an ER diagram), constraints, and indexes, please refer to the file below:

➡️ **[Detailed Database Schema Document](./docs/DATABASE.md)**

### **6. Technologies Used**

| Category | Technology / Tool |
| :--- | :--- |
| **Backend Framework** | ASP.NET Core 8.0, Python (FastAPI) |
| **Language** | C# 12, Python 3.10-slim |
| **Frontend** | WPF (.NET) |
| **Database** | PostgreSQL v15 |
| **ORM** | Entity Framework Core 8.0 |
| **Architecture** | Microservices, RESTful API |
| **Containerization** | Docker, Docker Compose |
| **Authentication** | JWT (JSON Web Tokens), ASP.NET Core Identity, OTP |
| **Monitoring** | Prometheus, Grafana, cAdvisor, Node Exporter |
| **Payment System** | Stripe SDK |
| **AI/ChatBot** | Ollama, Mistral 7B |
| **API Documentation** | Swagger (OpenAPI) |
| **Notifications** | SMTP, Notification.Wpf |
| **Visualization** | LiveCharts2 |

### **7. Key Systems and Workflows**

#### **7.1. User Authentication and Registration (OTP & JWT)**

##### **Step A: Initiate Registration and Send OTP**
1.  **Request:** The user sends their email, username, and password to the `POST /Auth/user/initiate-registration` endpoint.
2.  **Process:** The server checks for the uniqueness of the information. It generates a 6-digit OTP. The hashed version of this OTP and the user's information (including password) are temporarily saved in the `OtpVerifications` table. The plain OTP is sent to the user via email.
3.  **Response:** If successful, a message "OTP has been sent to your email address" is returned.

##### **Step B: Verify OTP and Complete Registration**
1.  **Request:** The user sends the code from their email to the `POST /Auth/user/verify-otp-and-register` endpoint.
2.  **Process:** The server verifies the OTP. If correct, it retrieves the user information from the temporary table and permanently saves it to the ASP.NET Identity system using `UserManager.CreateAsync()`. The user is assigned a default role, and the temporary OTP record is deleted.
3.  **Response:** A message "Registration successful. You can now log in" is returned.

##### **Step C: User Login and Token Generation**
1.  **Request:** The user sends a request to the `POST /Auth/user/login` endpoint with their email and password.
2.  **Process:** The credentials are verified using `SignInManager.CheckPasswordSignInAsync`. If successful, a **JWT Access Token** containing the user's ID, email, and roles is generated.
3.  **Response:** A `200 OK` response is returned with user information and the `accessToken`. This token is used in the `Authorization: Bearer <token>` header to access protected endpoints.

#### **7.2. Secure Debt System (GBS)**
1.  **Proposal:** The lender sends a debt proposal by entering the borrower's email.
2.  **Video Verification:** The borrower, upon accepting the proposal, records a **security video** containing a legal commitment statement.
3.  **Operator Approval:** An operator reviews the video and debt details via the `WinTrackManagerPanel`.
4.  **Encryption:** The approved video is irreversibly encrypted. A **20-character private key** to unlock the video is delivered only to the lender.
5.  **Automatic Tracking:** For overdue debts, the lender is granted access to the video and can decrypt it using their key.

#### **7.3. Multi-Format Reporting System**
A flexible system that allows users to convert their financial data into meaningful and portable documents.
1.  **Request:** The user selects the data range (date, accounts, categories, etc.) and the desired format (PDF, WORD, EXCEL, XML, TEXT, MARKDOWN) via the WPF application.
2.  **API Call:** The client sends a request with these criteria to the relevant reporting endpoint on FinTrackWebApi (e.g., `POST /api/reports/generate`).
3.  **Data Collection and Processing:** The server fetches the relevant financial data from the database based on the request.
4.  **Report Generation:** The fetched data is processed using format-specific libraries (e.g., QuestPDF for PDF, ClosedXML for Excel) and converted into a file stream.
5.  **Response:** The generated file stream is returned to the client with the appropriate `Content-Type` header. The client then presents this file to the user for download.

#### **7.4. Dynamic Exchange Rate Management**
An intelligent mechanism is used for data storage efficiency. The system compares the exchange rate data fetched from an external provider with existing data. If the change in the rate is below a significant threshold (e.g., the 6th decimal place), instead of creating a new record, a "multiplier" value of the existing record is updated. This optimizes the database size and improves query performance.

### **8. DevOps, Containerization, and Monitoring**

#### **8.1. Docker Architecture and Services**
All system components are containerized with **Docker**, ensuring portability, isolation, and easy deployment.

| Service Name | Technology/Purpose | Description |
| :--- | :--- | :--- |
| **fintrack_api** | ASP.NET Core | The Web API that executes the main business logic. |
| **finbot_api** | Python/FastAPI | The AI assistant service. |
| **wintrack_manager**| ASP.NET Core | The admin panel service. |
| **postgres_db** | PostgreSQL | The main application database (MainDB). |
| **postgres_db_logs**| PostgreSQL | The logging database (LogDB). |
| **ollama** | AI Engine | The AI engine that runs the Mistral 7B model. |
| **prometheus** | Monitoring | A time-series database that collects metrics. |
| **grafana** | Visualization | A dashboard for visualizing metrics. |
| **node_exporter** | Exporter | Collects health metrics from the API services. |
| **postgres_exporter**| Exporter | Collects health metrics from PostgreSQL. |
| **cadvisor** | Monitoring | Collects performance metrics of Docker containers. |
| **postgres_backup**| Backup | Automatically backs up MainDB every night at 03:00. |
| **ngrok** | Tunneling | Exposes the development API to the internet. |

#### **8.2. Monitoring and Health Checks (Prometheus & Grafana)**
*   **Prometheus:** Collects real-time performance metrics from all system components (APIs, databases, Docker) via `node_exporter`, `postgres_exporter`, and `cAdvisor`.
*   **Grafana:** Visualizes the data collected by Prometheus in interactive **dashboards**, showing the overall health of the system (CPU, RAM, API response times, etc.).

#### **8.3. Data Backup and Security**
The `postgres_backup` container takes a full backup of `MainDB` every night at 03:00, safeguarding the system against potential data loss.

### **9. Installation, Configuration, and Running**

#### **9.1. Prerequisites**
*   .NET SDK 8.0+
*   Docker Desktop
*   Git
*   Python 3.10-Slim

#### **9.2. Quick Setup with Docker (Recommended)**
1.  **Clone the Repository:**
    ```bash
    git clone https://github.com/EnesEfeTokta/FinTrackWebApi.git
    cd FinTrackWebApi
    ```
2.  **Start Services with Docker Compose:**
    ```bash
    docker-compose up -d
    ```
    This command will automatically start all microservices, databases, and dependencies.

#### **9.3. Configuration (`appsettings.json`)**
Sensitive configurations (API keys, passwords) are managed via the `appsettings.json` file and environment variables. For production, it is highly recommended to use environment variables or a secure secret management tool.

#### **9.4. Environment File Setup (`.env` for Docker Compose & FinBot)**

You need to create environment variable files for both Docker Compose and the FinBot service. These files store sensitive information and configuration values securely.

**A. Docker Compose `.env` File Example:**

Create a file named `.env` in the project root with the following content (replace values as needed):

```env
# --- PostgreSQL Main Database Settings ---
POSTGRES_DB=YOUR_POSTGRES_DB
POSTGRES_USER=YOUR_POSTGRES_USER
POSTGRES_PASSWORD=YOUR_POSTGRES_PASSWORD

# --- PostgreSQL Log Database Settings ---
LOG_DB_PASSWORD=YOUR_LOG_DB_PASSWORD

# --- Ngrok Service Settings ---
NGROK_AUTHTOKEN=YOUR_NGROK_AUTHTOKEN
```

**B. FinBot `.env` File Example:**

```env
FINTRACK_API_BASE_URL="http://localhost:8090"
```

**How to Use:**
1.  Copy the template for the main `.env` file into a new file named `.env` in the project's root directory.
2.  Replace all placeholder values (e.g., `your_db_user`) with your actual configuration secrets and keys.
3.  **Never commit `.env` files with real secrets to public repositories.** Ensure your `.gitignore` file includes `.env`.

Below is a reference for the `appsettings.json` structure, which is populated by these environment variables when running with Docker.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=Your_Port;Database=Your_DB;Username=Your_UserName;Password=Your_Password",
    "LogConnection": "Host=localhost;Port=Your_Port;Database=Your_DB;Username=Your_UserName;Password=Your_Password"
  },

  "Token": {
    "Issuer": "Your_Url",
    "Audience": "Your_Url",
    "SecurityKey": "Your_SecurityKey",
    "Expiration": 0
  },
  
  //... other settings
}
```
**Security Note:** For production, always use environment variables, Azure Key Vault, or similar secure configuration management tools for sensitive data.

### **10. API Usage and Testing**

*   **API Reference and Detailed Documentation**
The FinTrack API is documented with an interactive Swagger UI and detailed Markdown documents for each controller.

*   **Interactive API Documentation (Swagger)**
To test API endpoints and view schemas live, you can access the Swagger UI when the project is running at the following address:
*   FinTrackWebApi: http://localhost:5246/swagger

*   **Grafana**
This allows us to monitor the overall health of the system from a single central location. By default, the username and password are `admin`.
*   Grafana: http://localhost:3000

*   **Detailed Endpoint Documents**
Below are links to the detailed documentation files for each controller in the **FinTrackWebApi** service. Each document explains the endpoint's purpose, required request formats, and successful and error responses in detail.

*   [**`UserAuthController`**](./docs/api/UserAuthController.md) - Manages user registration, OTP verification, and login processes.
*   [**`UserController`**](./docs/api/UserController.md) - Provides all profile, settings, and summary data for the logged-in user from a single source.
*   [**`UserSettingsController`**](./docs/api/UserSettingsController.md) - Manages the user's profile, security, application, and notification settings.
*   [**`AccountController`**](./docs/api/AccountController.md) - Manages the user's financial accounts (bank, cash, etc.).
*   [**`TransactionCategoryController`**](./docs/api/TransactionCategoryController.md) - Manages personal categories for income/expense transactions.
*   [**`TransactionsController`**](./docs/api/TransactionsController.md) - Records and filters all income/expense transactions.
*   [**`BudgetsController`**](./docs/api/BudgetsController.md) - Creates and manages the user's budgets.
*   [**`ReportsController`**](./docs/api/ReportsController.md) - Generates dynamic financial reports in various formats (PDF, Excel, etc.).
*   [**`DebtController`**](./docs/api/DebtController.md) - Manages the main workflow of the Secure Debt System (GBS) (proposal, acceptance, default).
*   [**`VideosController`**](./docs/api/VideosController.md) - Manages video upload, encryption, and secure viewing processes for GBS.
*   [**`MembershipController`**](./docs/api/MembershipController.md) - Manages subscription plans and user memberships, initiates Stripe payment sessions.
*   [**`StripeWebhookController`**](./docs/api/StripeWebhookController.md) - Listens for successful payment events from Stripe and automatically activates memberships.
*   [**`NotificationController`**](./docs/api/NotificationController.md) - Manages user-specific in-app notifications.
*   [**`FeedbackController`**](./docs/api/FeedbackController.md) - Allows users to submit feedback.
*   [**`ChatController`**](./docs/api/ChatController.md) - Acts as a secure proxy for the FinBot (Python) service.
*   [**`LogController`**](./docs/api/LogController.md) - Provides (secured) access to system log files.

### **Project Documentation**
This project is supported by comprehensive documentation aimed at various technical levels. Use the links below to quickly find the information you need.

*   ➡️ **[System Architecture](./docs/ARCHITECTURE.md)**
    *   Read this document to understand the project's high-level architecture, services, technologies, and the data flow between them.

*   ➡️ **[Detailed Database Schema](./docs/DATABASE.md)**
    *   Look here for a detailed description of the database tables, columns, relationships, and the ERD.

*   ➡️ **[API Reference and Endpoint Details](./docs/api/)**
    *   Markdown files containing technical details, request/response formats, and usage examples for all API endpoints are collected here.

*   ➡️ **[Frequently Asked Questions (FAQ)](./docs/FAQ.md)**
    *   Review this document for quick answers on setting up, running the project, API usage, and common issues.

*   ➡️ **[Roles and Permissions Matrix](./docs/ROLES_AND_PERMISSIONS.md)**
    *   Use this document to understand the user roles in the system (`User`, `Admin`, etc.) and their permissions on the API.

### **11. Conclusion and Future Vision**

FinTrack is not just a financial tracking tool but a holistic ecosystem aimed at enhancing the financial well-being of its users. With its solid technical foundation, modern microservices architecture, and innovative features, it is ready to meet market needs. The future vision includes **mobile applications (iOS & Android)**, **advanced AI features** (anomaly detection, proactive budget optimization), and **third-party integrations**.

### **12. License and Contact**

*   **License:** This project is licensed under the GPL license. See the [LICENSE](LICENSE) file for details.
*   **Project Owner:** Enes Efe Tokta
*   **Contact:** [enesefetokta@gmail.com](mailto:enesefetokta@gmail.com)
*   **LinkedIn:** [https://www.linkedin.com/in/enes-efe-tokta/](https://www.linkedin.com/in/enes-efe-tokta/)
*   **Project Link:** [https://github.com/EnesEfeTokta/FinTrackWebApi](https://github.com/EnesEfeTokta/FinTrackWebApi)