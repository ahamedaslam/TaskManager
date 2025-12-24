
---

# Task Manager - Tenant API

A scalable and secure **multi-tenant task management system** built with **ASP.NET Core**. This project is designed to handle multiple organizations (tenants) while ensuring strict data isolation and role-based security.

## 🚀 Overview

The Task Manager API streamlines collaboration by allowing users to manage tasks effectively across different clients or departments. It addresses enterprise-level challenges such as secure data separation, role-based access control (Admin vs. Normal User), and system maintainability through clean architecture.

## 🛠 Technology Stack

* 
**Backend:** ASP.NET Core Web API (.NET 7/8) 


* 
**Database:** SQL Server with Entity Framework Core 


* 
**Frontend:** Angular 


* 
**Security:** JWT Authentication, ASP.NET Identity, and OTP Verification 


* 
**Logging:** Serilog with structured logging 


* 
**Documentation:** Swagger / OpenAPI 



## 🏗 Architecture & Patterns

The project follows industry best practices to ensure a clean and modular codebase:

* 
**Layered Architecture:** Separate layers for Controllers, Services, Repositories, and Middleware.


* 
**Repository & Service Patterns:** Decouples business logic from data persistence.


* 
**Multi-Tenancy:** A dedicated user-tenant linking system to scope data access.


* 
**Middleware:** Custom implementation for global exception handling and JWT processing.



## ✨ Key Features

* 
**Multi-Tenant Isolation:** Data is strictly separated between tenants.


* **Role-Based Access Control (RBAC):**
* 
**Admin:** Full task management capabilities.


* 
**Normal User:** Restricted to viewing and updating their own tasks.




* 
**Advanced Task Management:** Support for CRUD operations, filtering, sorting, and pagination.


* 
**AI Chatbot:** An integrated assistant that answers task-related queries and provides summaries while respecting tenant permissions.


* 
**Structured Logging:** Every log entry includes a unique `logId` for easier debugging.



## 🛣 API Endpoints (Quick Reference)

| Method | Endpoint | Description |
| --- | --- | --- |
| `POST` | `/api/login` | Authenticate and receive JWT.

 |
| `POST` | `/api/Tenant` | Register a new tenant.

 |
| `GET` | `/api/taskmanager` | Get tasks (filtered/paged).

 |
| `POST` | `/api/taskmanager` | Create a new task.

 |
| `PATCH` | `/api/taskmanager/completion-status` | Toggle task status.

 |

## 📂 Project Structure

The solution is organized into logical components to ensure scalability:

* **Controllers:** API Endpoints.
* **Services:** Business Logic & AI Integration.
* **Repositories:** Data Access.
* **Models/DTOs:** Data structures and transfer objects.
* 
**Helpers:** Standardized response utilities (`ResponseHelper.cs`).



---

---

**Would you like me to add a "Installation & Setup" section with specific commands for .NET and SQL Server?**
