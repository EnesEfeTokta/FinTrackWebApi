# **Changelog**

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## **Announcing the Stable Release!**

We are excited to announce the official release of **FinTrack version 1.0.0!** This marks the project's first stable release, featuring a complete and robust set of core functionalities ready for use in a development environment. Thank you for following our progress.

---

## [Unreleased]

_This section contains changes that will be in the next release but have not been published yet._

### Added
- Feature to add notes to `Transaction` entities.
- `/api/profile` endpoint for user profiles.

### Changed
- Improved logging within the `AccountController` for more detailed error tracking.
- Updated token expiration time to 1 hour for enhanced security.

### Fixed
- Fixed a bug that caused incorrect account balance calculation when a transaction was deleted.
- Fixed an issue where the `DELETE /api/categories/{id}` endpoint returned a `500 Internal Server Error` when trying to delete a category with associated transactions. It now correctly returns a `400 Bad Request` with an informative message.

---

## [1.0.0] - 2023-11-15

### Added
- **Initial Stable Release of the Project!**
- Full development environment support with Docker and Docker Compose.
- Basic CRUD (Create, Read, Update, Delete) operations for `FinTrackWebApi` (.NET 8):
  - User Authentication (`/api/auth`) - Login and Register.
  - Account Management (`/api/accounts`).
  - Category Management (`/api/categories`).
  - Budget Management (`/api/budgets`).
  - Transaction Management (`/api/transactions`).
- Initial structure and integration for `FinBotWebApi` (Python).
- PostgreSQL database integration.
- Created core project documentation: `README.md`, `FAQ.md`, `CONTRIBUTING.md`, and this `CHANGELOG.md` file.
- Interactive API documentation with Swagger UI (`/swagger`).

---

## [0.2.0] - 2023-10-20

### Added
- Implemented JWT (JSON Web Token) based authorization system. All sensitive endpoints are now protected.
- Added controllers and business logic for `Budgets` and `Transactions`.

### Changed
- Upgraded the project from .NET 7 to .NET 8.

### Fixed
- Fixed an issue where category names were case-sensitive, allowing duplicate names with different casing.

---

## [0.1.0] - 2023-09-30

### Added
- Initial project setup.
- Created the basic structure for the `FinTrackWebApi` project.
- Added basic, non-authorized CRUD endpoints for `Account` and `Category` models.
- Configured PostgreSQL as the database and created initial migrations with Entity Framework Core.