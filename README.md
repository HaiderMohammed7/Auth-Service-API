# Auth Service API

A centralized Authentication & Authorization API built with ASP.NET Core and Clean Architecture.

The project provides a secure and reusable authentication system that can be integrated with multiple applications. It supports JWT authentication, refresh token rotation, role-based authorization, password management, and security protections against common attacks.

---

## Features

### Authentication

* User Registration
* User Login
* User Logout
* Logout From All Devices
* Current User Information (`/me`)

### Token Management

* JWT Access Tokens
* Refresh Tokens
* Refresh Token Rotation
* Secure Token Revocation

### Password Management

* Change Password
* Forgot Password
* Reset Password

### Security

* PBKDF2 Password Hashing
* Role-Based Authorization
* Rate Limiting
* Brute Force Protection
* Account Lockout After Failed Login Attempts
* Global Exception Handling
* Request Logging Middleware

---

## Architecture

The project follows Clean Architecture principles.

```text
AuthService.API
│
├── Controllers
├── Middleware
│
AuthService.Application
│
├── Interfaces
├── Services
├── Exceptions
│
AuthService.Domain
│
├── Entities
├── Enums
│
AuthService.Infrastructure
│
├── Data
├── Repositories
├── Migrations
│
AuthService.Shared
│
├── DTOs
├── Helpers
├── Responses
```

### Layers

#### API Layer

Handles HTTP requests, responses, middleware, and endpoint exposure.

#### Application Layer

Contains business logic, use cases, service contracts, and application rules.

#### Domain Layer

Contains core entities and business models.

#### Infrastructure Layer

Handles data access, repositories, database operations, and migrations.

#### Shared Layer

Contains DTOs, helper classes, and standardized API responses.

---

## Available Endpoints

| Method | Endpoint                  |
| ------ | ------------------------- |
| POST   | /api/auth/register        |
| POST   | /api/auth/login           |
| POST   | /api/auth/refresh         |
| GET    | /api/auth/me              |
| POST   | /api/auth/logout          |
| POST   | /api/auth/logout-all      |
| POST   | /api/auth/change-password |
| POST   | /api/auth/forgot-password |
| POST   | /api/auth/reset-password  |

---

## Technologies

* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* JWT Bearer Authentication
* Swagger / OpenAPI
* Clean Architecture
* Dependency Injection

---

## Security Design

The authentication flow follows modern security practices:

1. User logs in using email and password.
2. API issues an Access Token and Refresh Token.
3. Access Token is used for protected endpoints.
4. Refresh Token Rotation generates a new refresh token on every refresh request.
5. Logout revokes active refresh tokens.
6. Failed login attempts are tracked and can trigger account lockout.

---

## Project Status

### Current Version

v1.2.0

### Completed

* Authentication System
* Authorization System
* Refresh Token Rotation
* Password Management
* Security Hardening
* Clean Architecture Implementation

### Future Enhancements

* Email Service Integration
* Two-Factor Authentication (2FA)
* Docker Support
* CI/CD Pipeline

---

## API Documentation

Swagger UI is included for testing and exploring all available endpoints.

---
