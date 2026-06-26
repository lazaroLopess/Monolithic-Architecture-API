# Monolithic Architecture API

A Web API built with **ASP.NET Core** following the **Monolithic Architecture** pattern. This project was created for learning purposes while also serving as a **semi-ready template** for building new APIs.

The goal is to provide a solid foundation with the most common infrastructure already implemented, allowing future projects to focus primarily on business logic.

---

# Features

Currently, the project includes:

- ✅ JWT Authentication
- ✅ Refresh Token Authentication
- ✅ ASP.NET Core Identity
- ✅ User Management
- ✅ Role Management
- ✅ Role-Based Authorization
- ✅ Entity Framework Core
- ✅ MySQL Support
- ✅ Entity Framework Migrations
- ✅ Global Exception Handling Middleware
- ✅ Standardized API Responses
- ✅ Swagger / OpenAPI Documentation
- ✅ Custom Logging Provider
- ✅ DTO Mapping
- ✅ Clean project organization

---

# Project Structure

```
Context/
Controllers/
DTOs/
    Requests/
    Responses/
Identity/
Logging/
Mapping/
Middleware/
Migrations/
TokenServices/
Program.cs
```

Each folder has a single responsibility, making the project easier to maintain, understand, and extend.

---

# Authentication

Authentication is implemented using:

- ASP.NET Core Identity
- JSON Web Tokens (JWT)
- Refresh Tokens

The authentication flow supports:

- User Registration
- User Login
- Access Token Generation
- Refresh Token Generation
- Access Token Renewal

---

# User Management

The project already provides a complete User Controller with features such as:

- User registration
- Authentication
- User retrieval
- User updates
- Identity integration

---

# 🛡️ Roles & Authorization

Role-based authorization is fully implemented using ASP.NET Core Identity.

Examples of supported roles:

- Administrator
- User
- Moderator

Endpoints can be protected using the built-in authorization attributes provided by ASP.NET Core.

---

# Standard API Responses

All endpoints return standardized responses.

Example:

```json
{
  "message": "Login successful",
  "statusCode": 200,
  "data": {
    "token": "...",
    "refreshToken": "...",
    "accessTokenExpiresAt": "...",
    "refreshTokenExpiresAt": "..."
  }
}
```

---

# Technologies

- ASP.NET Core
- .NET
- C#
- Entity Framework Core
- ASP.NET Core Identity
- JWT Authentication
- MySQL
- Pomelo Entity Framework Provider
- Swagger / OpenAPI

---

# Project Purpose

This project is intended to be used as a **semi-ready API template**.

Instead of starting every project from scratch, the core infrastructure has already been implemented, including:

- Authentication
- Authorization
- User Management
- Role Management
- JWT Configuration
- Identity Configuration
- Entity Framework Configuration
- Global Exception Handling
- Logging
- DTO Structure
- Entity Mapping
- Standard API Responses

This allows new projects to focus mainly on implementing business rules.

---

# Planned Features

The following features will be added in future updates:

- 📌 Audit Controller
- 📌 Memory Cache
- 📌 Rate Limiting
- 📌 API Versioning
- 📌 Advanced Validation
- 📌 Structured Logging
- 📌 Pagination
- 📌 Dynamic Filtering
- 📌 Unit Tests
- 📌 Integration Tests

---

# ▶Getting Started

### Clone the repository

```bash
git clone https://github.com/lazaroLopess/Monolithic-Architecture-API.git
```

### Configure the application settings

Before running the project, create an `appsettings.Development.json` file in the project root.

The `appsettings.Development.json` file is intentionally ignored by Git because it contains environment-specific settings, such as the database connection string and JWT configuration.

Use the `appsettings.json` file as a reference and keep the same structure, replacing the values with your own local configuration.

For the database connection, **follow the same connection string format used in `appsettings.json`**, changing only the server, database name, username, password, or any other values required by your local environment.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=YourDatabase;Uid=your_user;Pwd=your_password;"
  },
  "JWT": {
    "SecretKey": "your_secret_key",
    "Issuer": "your_issuer",
    "Audience": "your_audience",
    "AccessTokenExpirationHours": 1,
    "RefreshTokenExpirationHours": 7
  }
}
```

### Apply database migrations

```bash
dotnet ef database update
```

### Run the project

```bash
dotnet run
```

### Open Swagger

```
https://localhost:<port>/swagger
```

---

# Target Audience

This project is intended for:

- Developers learning ASP.NET Core
- Students studying Web API development
- Developers looking for a reusable API starter template
- Anyone who wants a pre-configured authentication and authorization infrastructure

---

# Contributing

Contributions, suggestions, and improvements are always welcome.

Feel free to fork the repository, open issues, or submit pull requests.

---

# License

This project is licensed under the MIT License.

---

# Created By

**Lázaro Lopes**
Software Developer | .NET & ASP.NET Core Enthusiast

If you found this project useful, consider giving it a ⭐ on GitHub.