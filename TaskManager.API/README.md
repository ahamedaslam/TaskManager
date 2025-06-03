# Task Manager API

Task Manager API is a .NET 8 Web API for user authentication and role-based access management. It leverages ASP.NET Core Identity, JWT authentication, and Entity Framework Core for secure and scalable user management.

## Features
- User registration with role assignment (Admin, Normal)
- JWT-based authentication
- Role-based authorization
- Swagger/OpenAPI documentation
- Extensible repository pattern

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (or compatible connection string)

### Setup
1. Clone the repository:
   ```bash
   git clone <your-repo-url>
   cd TaskManager.API
   ```
2. Update the connection string in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "TaskManagerAuthDB": "<Your SQL Server connection string>"
   }
   ```
3. Apply database migrations:
   ```bash
   dotnet ef database update --project TaskManager.API
   ```
4. Run the API:
   ```bash
   dotnet run --project TaskManager.API
   ```

### API Usage
- Access Swagger UI at `https://localhost:<port>/swagger` for interactive API documentation.
- Register a new user via `POST /api/Auth/register` with JSON body:
  ```json
  {
    "username": "user@example.com",
    "password": "yourpassword",
    "roles": ["Normal"]
  }
  ```

## Project Structure
- `Controllers/` – API endpoints
- `DBContext/` – Entity Framework Core context
- `DTOs/` – Data transfer objects
- `Repository/` – Repository pattern implementations
- `Models/` – Response and domain models

## Security
- Password policy is configurable in `Program.cs`
- JWT settings are configured in `appsettings.json`

## License
This project is licensed under the MIT License.
