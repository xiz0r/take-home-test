# Fundo Loans MVP

A full-stack loan management application built with **.NET 10** (Backend) and **Angular 19** (Frontend), following **Hexagonal Architecture** and **Domain-Driven Design (DDD)** principles.

---

## 🚀 Features

### API Endpoints
| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/loans` | Create a new loan |
| `GET` | `/loans` | Retrieve all loans |
| `GET` | `/loans/{id}` | Retrieve a specific loan by ID |
| `POST` | `/loans/{id}/payment` | Make a payment on a loan |
| `POST` | `/auth/token` | Generate JWT authentication token |

### Backend Features
- **RESTful API** with ASP.NET Core Web API
- **JWT Authentication** for secure API access
- **Swagger/OpenAPI** documentation (available in Development mode)
- **Entity Framework Core** with SQL Server database
- **Seed Data** for initial database population
- **Error Handling Middleware** for consistent error responses
- **Structured Logging** with Microsoft.Extensions.Logging
- **Docker Support** with Dockerfile and Docker Compose
- **GitHub Actions** CI/CD pipeline for build and test automation

### Architecture
- **Hexagonal Architecture** (Ports & Adapters) for clean separation of concerns
- **Domain-Driven Design (DDD)** patterns
  - Aggregate Root (`Loan`) with encapsulated business logic
  - Repository pattern for data access abstraction
  - Use Cases for application layer orchestration
- Simplified approach: validations are kept in the `Loan` aggregate for code simplicity (no separate Value Objects)

---

## 📁 Project Structure

```
├── backend/
│   └── src/
│       ├── Fundo.Domain/           # Domain layer (Entities, Enums, Repository interfaces)
│       ├── Fundo.Application/      # Application layer (Use Cases, DTOs)
│       ├── Fundo.Infrastructure/   # Infrastructure layer (Persistence, Repository implementations)
│       ├── Fundo.Applications.WebApi/  # Presentation layer (Controllers, Middleware)
│       └── Fundo.Services.Tests/   # Unit and Integration tests
│
└── frontend/                       # Angular 19 frontend application
```

---

## 🛠️ Tech Stack

### Backend
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- xUnit (Testing)
- Docker

### Frontend
- Angular 19
- TypeScript

---

## 🏃 Getting Started

### Prerequisites
- .NET SDK 10
- Node.js (for frontend)
- Docker (optional, for containerized setup)

### Backend Setup

#### Option 1: Local Development (without Docker)

1. Navigate to the backend source folder:
   ```sh
   cd backend/src
   ```

2. Build the solution:
   ```sh
   dotnet build
   ```

3. Run the API:
   ```sh
   cd Fundo.Applications.WebApi
   dotnet run
   ```

4. Verify the API is running:
   ```
   GET http://localhost:5000/loans
   ```

#### Option 2: Docker Compose

1. From the `backend/src` directory:
   ```sh
   docker compose up --build
   ```

2. Access the API at:
   ```
   http://localhost:8080/loans
   ```

### Frontend Setup

1. Navigate to the frontend folder:
   ```sh
   cd frontend
   ```

2. Install dependencies:
   ```sh
   npm install
   ```

3. Start the development server:
   ```sh
   npm start
   ```

4. Open `http://localhost:4200/` in your browser.

---

## 📖 API Documentation (Swagger)

Swagger UI is available **only in Development mode**:

| Environment | Swagger UI | OpenAPI JSON |
|-------------|------------|--------------|
| Local | http://localhost:5000/ | http://localhost:5000/swagger/v1/swagger.json |
| Docker | http://localhost:8080/ | http://localhost:8080/swagger/v1/swagger.json |

To enable Swagger, run with:
```sh
ASPNETCORE_ENVIRONMENT=Development dotnet run
```

---

## 🧪 Testing

### Run All Tests
```sh
cd backend/src
dotnet test
```

### Test Coverage
- **Unit Tests**: Use Cases (LoanCreator, LoanFinder, LoanListFinder, LoanPaymentMaker)
- **Integration Tests**: API endpoints with TestWebApplicationFactory

---

## 🔐 Authentication

The API uses **JWT Bearer Authentication**. To access protected endpoints:

1. Obtain a token via `POST /auth/token`
2. Include the token in requests: `Authorization: Bearer <token>`

> ⚠️ **Note**: For simplicity and to keep this MVP self-contained, the `/auth/token` endpoint generates tokens without requiring an external Identity Provider (IdP) or real credential validation. **This is not secure for production use.** In a real-world application, you should integrate with a proper authentication provider (e.g., Auth0, Azure AD, IdentityServer, etc.).

---

## 🐳 Docker Services

| Service | Description | Port |
|---------|-------------|------|
| `fundo-api` | .NET Web API | 8080 |
| `fundo-sqlserver` | SQL Server 2022 Express | 1433 |

---

## 📝 Domain Model

### Loan Entity
| Property | Type | Description |
|----------|------|-------------|
| `Id` | GUID | Unique identifier |
| `Amount` | Decimal | Original loan amount |
| `CurrentBalance` | Decimal | Remaining balance |
| `ApplicantName` | String | Borrower's name |
| `Status` | Enum | Active / Paid |
| `CreatedAt` | DateTime | Creation timestamp |
| `UpdatedAt` | DateTime? | Last update timestamp |

### Business Rules
- Loan amount must be greater than zero
- Applicant name is required
- Payment amount must be positive and cannot exceed current balance
- Loan status changes to `Paid` when balance reaches zero
- Cannot make payments on already paid loans


