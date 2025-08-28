# **FinTrack Project - Frequently Asked Questions (FAQ)**

This document is designed to provide quick and clear answers to frequently asked questions about the FinTrack project, its architecture, API usage, and development environment.

## Table of Contents
*   [General and Getting Started](#general-and-getting-started)
*   [API Architecture and Usage](#api-architecture-and-usage)
*   [Authentication & Authorization](#authentication--authorization)
*   [Development Environment and DevOps](#development-environment-and-devops)
*   [Database](#database)

---

## General and Getting Started

### **Question:** What is this project and what is its main purpose?
**Answer:** FinTrack is a SaaS (Software as a Service) platform that enables individual and professional users to manage their financial lives. This repository contains the **backend services** of the platform, providing a secure and rich API infrastructure for client applications (WPF, mobile, etc.).

### **Question:** Where can I find the main documentation for the project?
**Answer:** All technical details of the project are centralized in the `docs/` folder:
1.  **`README.md`:** Provides a general overview of the project, the technology stack, and quick start information.
2.  **`docs/ARCHITECTURE.md`:** Details the high-level architecture of the system, its services, and data flows.
3.  **`docs/DATABASE.md`:** Contains the database schema, tables, relationships, and an ERD.
4.  **`docs/api/` Folder:** Houses detailed endpoint documentation for each API controller.
5.  **Swagger (Interactive UI):** An interface, accessible at `http://localhost:5246/swagger` when the project is running, that allows you to test endpoints live.

### **Question:** What is the current status of the project?
**Answer:** The project is under active development. The `README.md` file displays the latest build status of the main branch.

---

## API Architecture and Usage

### **Question:** What is the base URL for the API?
**Answer:** The default address for services running in the local development environment with Docker is: `http://localhost:5246`.

### **Question:** What date and time format should be used in the API?
**Answer:** The **ISO 8601** standard (`YYYY-MM-DDTHH:mm:ssZ`) is used throughout the API. All date and time data should be sent to the server in **UTC** (Coordinated Universal Time) and will be received from the server in this format.

### **Question:** Why are fields like `Status` and `Type` sent as text (`"Active"`, `"Income"`) instead of numbers?
**Answer:** This is a deliberate design choice to enhance the readability and developer-friendliness of the API. ASP.NET Core automatically converts these string values to the correct `enum` types on the server side. This way, a developer using the API doesn't need to memorize what the numbers mean.

### **Question:** Why does a `DELETE` operation return a `204 No Content`?
**Answer:** This is not an error. The `204 No Content` HTTP status code indicates that the operation (e.g., deletion) was completed successfully, but the server has no content to return in the response body. This is a common and correct approach in RESTful API design.

---

## Authentication & Authorization

### **Question:** How do I authenticate to access the API?
**Answer:** Through a two-step process:
1.  **Registration:** You need to complete an OTP-verified registration process using the `POST /UserAuth/initiate-registration` and `POST /UserAuth/verify-otp-and-register` endpoints.
2.  **Login:** After registration, you can obtain a **JWT (JSON Web Token)** by sending your email and password to the `POST /UserAuth/login` endpoint.

### **Question:** How should I use the JWT I received?
**Answer:** You must include the `accessToken` in the `Authorization` HTTP header of all requests that require authorization, prefixed with `Bearer `. **Example:** `Authorization: Bearer eyJhbGciOiJIUzI1Ni...`

### **Question:** What is the difference between `401 Unauthorized` and `403 Forbidden`?
**Answer:**
*   **401 Unauthorized:** This means your identity could not be verified. You are considered "not logged in" to the system. It is typically received when the token is missing, invalid, or has expired.
*   **403 Forbidden:** This means your identity has been verified—you are "logged in"—but you do not have permission to access the requested resource or perform the action. For example, a user with the `User` role attempting to access an endpoint that is only accessible to the `Admin` role will receive this error.

---

## Development Environment and DevOps

### **Question:** How can I run the project on my local machine?
**Answer:** The **only and recommended way to run the project is with Docker**. Simply run the `docker-compose up --build` command in the project's root directory. This command will automatically start all services (APIs, databases, monitoring tools) with the correct configurations.

### **Question:** Why is using Docker mandatory?
**Answer:** Docker packages all of the project's dependencies (specific .NET and Python versions, PostgreSQL, Ollama, etc.) into isolated containers. This completely eliminates the "it worked on my machine" problem and ensures that every developer works in the exact same environment.

### **Question:** How can I view the logs of a specific service?
**Answer:** Open a new terminal and use the `docker-compose logs -f <service_name>` command. **Example:** `docker-compose logs -f fintrack_api`

### **Question:** How can I access the monitoring dashboard (Grafana)?
**Answer:** While the project is running, navigate to `http://localhost:3000` in your browser. Grafana provides dashboards that show the overall health of the system, CPU/RAM usage, and API performance.

### **Question:** Where should I store sensitive information (API keys, passwords)?
**Answer:** **Never write them directly into `appsettings.json`!** For local development, use ASP.NET Core's "User Secrets" feature. For production environments, provide this information via **environment variables** or a secure configuration management tool like Azure Key Vault.

---

## Database

### **Question:** How can I connect to the development database with a client (DBeaver, pgAdmin)?
**Answer:** In the `docker-compose.yml` file, the database port (`5432`) is mapped to port `5432` on your machine. You can connect using the following information:
*   **Host:** `localhost`
*   **Port:** `5432`
*   **Database (MainDB):** `fintrack_main_db`
*   **Username:** `postgres`
*   **Password:** `your_strong_password` (check the `docker-compose.yml` file or your `.env` file)

### **Question:** What is the difference between `MainDB` and `LogDB`?
**Answer:**
*   **`MainDB`:** The primary database where the main application data (users, accounts, etc.) is stored.
*   **`LogDB`:** A secondary database that records every data modification (Create, Update, Delete) on `MainDB` for auditing purposes. This provides full traceability.

### **Question:** Will my database data be deleted if I run the `docker-compose down` command?
**Answer:** **No, it will not.** Your database data is stored in Docker "volumes" named `postgres_data` and `postgres_log_data`. These volumes persist your data even if the containers are stopped and removed. If you want to completely reset the data, you need to use the `docker-compose down -v` command.