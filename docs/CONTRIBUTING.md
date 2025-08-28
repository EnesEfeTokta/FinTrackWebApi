# **Guide to Contributing to the FinTrack Project**

First off, thank you for considering contributing to the FinTrack project! This project is made stronger by community contributions. This guide is designed to make the contribution process as easy and transparent as possible.

## Table of Contents
- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
  - [Reporting Bugs](#reporting-bugs)
  - [Suggesting New Features or Enhancements](#suggesting-new-features-or-enhancements)
  - [Your First Contribution](#your-first-contribution)
- [Development Environment Setup](#development-environment-setup)
- [Contribution Process and Git Workflow](#contribution-process-and-git-workflow)
- [Coding Standards](#coding-standards)
  - [General Rules](#general-rules)
  - [Standards for C# (.NET)](#standards-for-c-net)
  - [Standards for Python](#standards-for-python)
- [Commit Message Standards](#commit-message-standards)
- [Pull Request (PR) Process](#pull-request-pr-process)

## Code of Conduct

All participants in this project are expected to adhere to our Code of Conduct, outlined in the `CODE_OF_CONDUCT.md` file. Please read and follow these rules to ensure a friendly and inclusive environment for everyone.

## How Can I Contribute?

### Reporting Bugs
If you find a bug, please create a new "issue" in the GitHub "Issues" section. When describing your bug, try to include the following information:
- A clear and concise summary of what the bug is.
- A list of steps required to reproduce the bug.
- An explanation of what you expected to happen.
- A description of what actually happened.
- Screenshots, if possible.

### Suggesting New Features or Enhancements
Have a great idea? Share it with us by creating a new "issue" in the GitHub "Issues" section. Explain your suggestion in detail and describe why it would be beneficial.

### Your First Contribution
If you are contributing to the project for the first time, you can browse issues tagged with `good first issue` or `help wanted` in the "Issues" section. These are typically more suitable for getting started with the project.

## Development Environment Setup

The project is fully based on **Docker** to standardize and simplify the development environment.

**Requirements:**
- [Git](https://git-scm.com/)
- [Docker](https://www.docker.com/products/docker-desktop/) and Docker Compose

**Setup Steps:**
1.  **Fork** this repository and then clone your fork to your local machine:
    ```bash
    git clone https://github.com/YOUR_USERNAME/FinTrack.git
    cd FinTrack
    ```
2.  In the project's root directory, run the following command to start all services (APIs and database):
    ```bash
    docker-compose up --build -d
    ```
    - The `--build` flag ensures that the images are rebuilt to reflect any changes you've made to the code.
    - The `-d` flag runs the services in the background (detached mode).
3.  That's it! Your services are now running.
    - **Main API:** `http://localhost:5246`
    - **Swagger UI:** `http://localhost:5246/swagger`
    - **Bot API:** `http://localhost:8000`

You can stop the services using the `docker-compose down` command.

## Contribution Process and Git Workflow

1.  Fork and clone the repository as described above.
2.  Create a new branch from the main development branch (`main` or `develop`). Your branch name should summarize the work you are doing.
    ```bash
    # Examples:
    git checkout -b feature/add-user-profile-endpoint
    git checkout -b fix/login-validation-bug
    ```
3.  Make your changes and ensure they adhere to the coding standards.
4.  Commit your changes with meaningful commit messages.
5.  Push your new branch to your own fork:
    ```bash
    git push origin feature/add-user-profile-endpoint
    ```
6.  Open a **Pull Request (PR)** from your fork to the original FinTrack repository on GitHub.

## Coding Standards

### General Rules
- All code, comments, and documentation must be in **English**.
- Keep your code clean and understandable. Avoid unnecessary complexity.

### Standards for C# (.NET)
- Follow [Microsoft's C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).
- Use the project's `ILogger` for debugging instead of `Console.WriteLine`.
- Use `async/await` correctly for asynchronous operations.
- Add XML comments for all public methods and classes. This is also required for Swagger documentation.

### Standards for Python
- Adhere to the [PEP 8 Style Guide](https://www.python.org/dev/peps/pep-0008/).
- It is recommended to format your code with a formatter like `black` before submitting.

## Commit Message Standards

Our project uses the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) standard for commit messages. This makes the Git history readable and helps automate the versioning process.

**Format:** `<type>(<scope>): <description>`

**Common Types:**
- `feat`: When adding a new feature.
- `fix`: When fixing a bug.
- `docs`: When changing only the documentation.
- `style`: Formatting changes that do not affect the meaning of the code (whitespace, semicolons, etc.).
- `refactor`: Code restructuring that neither fixes a bug nor adds a feature.
- `test`: When adding missing tests or correcting existing tests.
- `chore`: Changes that affect the build process, helper tools, or libraries.

**Examples:**
```
feat(auth): Add password reset functionality
fix(account): Correctly calculate account balance with negative transactions
docs(readme): Update setup instructions for Docker
```

## Pull Request (PR) Process

1.  The title of your PR should clearly summarize the change you've made (e.g., `feat(categories): Add support for sub-categories`).
2.  In the PR description, answer the following questions:
    - **Why is this change necessary?**
    - **What does it do?**
    - **Is there a related "Issue" number?** (e.g., `Closes #123`)
3.  Ensure that your PR serves a single purpose. Do not combine multiple unrelated changes into a single PR.
4.  Make sure the code you submit builds the project successfully and that all tests pass.
5.  Be prepared to make requested changes and respond to feedback after your PR has been reviewed.

Thank you again for helping make FinTrack a better place