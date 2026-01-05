# \# Auth Service API

# 

# Centralized Authentication \& Authorization API built with ASP.NET Core.

# 

# \## Features

# \- JWT Authentication

# \- Refresh Token with Rotation

# \- Secure Password Hashing (PBKDF2)

# \- Role-based Authorization

# \- Login / Logout / Refresh

# \- Rate Limiting \& Brute-force Protection

# \- Global Error Handling

# \- Swagger Documentation

# 

# \## Architecture

# \- API Layer

# \- Application (Business Logic)

# \- Domain (Entities)

# \- Infrastructure (Planned)

# \- Shared (DTOs \& Responses)

# 

# \## Authentication Flow

# 1\. Login with email \& password

# 2\. Receive Access Token + Refresh Token

# 3\. Use Access Token for protected endpoints

# 4\. Refresh Access Token using Refresh Token

# 5\. Logout revokes refresh tokens

# 

# \## Security

# \- PBKDF2 password hashing

# \- JWT with claims \& roles

# \- Refresh token rotation

# \- Rate limiting on sensitive endpoints

# \- Account lockout after failed attempts

# 

# \## Technologies

# \- ASP.NET Core Web API

# \- JWT Bearer Authentication

# \- Swagger

# \- SQL Server (Schema designed)

# \- Clean Architecture

# 

# \## Status

# v1.0.0 – Core authentication system completed.

# Infrastructure layer will be implemented in a future update.



