# dotnet-idempotent-bulk-api

Idempotent POST Web API sample built with **.NET 8**, **ASP.NET Core**, **MediatR (CQRS)**, **EF Core**, and **MySQL**.
Runs locally via **Docker Compose** and provides Swagger UI for quick verification.

## Features

- **Idempotent POST** using `Idempotency-Key` header
  - Same key returns the same response without creating duplicates
  - Handles race conditions with a unique index + retry/exception handling
- CQRS-style structure with MediatR (`Command/Query + Handler`)
- MySQL 8 (Docker)
- Swagger UI

## Tech Stack

- .NET 8 / ASP.NET Core Web API
- MediatR
- Entity Framework Core
- Pomelo.EntityFrameworkCore.MySql
- MySQL 8
- Docker / Docker Compose

## Project Structure

- **BulkApi.Api**: Controllers, HTTP DTOs, DI, Swagger
- **BulkApi.Application**: Commands/Queries + Handlers (UseCases)
- **BulkApi.Infrastructure**: AppDbContext, EF Core config, MySQL
- **BulkApi.Domain**: Entities, domain rules
- **docker-compose.yml**

## Getting Started

### 1. Run with Docker Compose

```bash
docker compose up -d
docker compose ps
```

### 2. Open Swagger

Go to: http://localhost:8080/swagger

### 3. Database Migration (EF Core)

If the tables are not created yet, apply migrations using one of the following methods.

**Option A: .NET CLI**

First, install the tool if you haven't:
```bash
dotnet tool install --global dotnet-ef
```

Run migrations:
```bash
dotnet ef migrations add CreateInitialTables --project BulkApi.Infrastructure --startup-project BulkApi.Api
dotnet ef database update --project BulkApi.Infrastructure --startup-project BulkApi.Api
```

**Option B: Visual Studio (Package Manager Console)**

- **Default project**: `BulkApi.Infrastructure`
- **Startup project**: `BulkApi.Api`

Run:
```powershell
Add-Migration CreateInitialTables -Project BulkApi.Infrastructure -StartupProject BulkApi.Api
Update-Database -Project BulkApi.Infrastructure -StartupProject BulkApi.Api
```

## API Usage

### Create Receipt (Idempotent POST)

**Header**
`Idempotency-Key`: <any-unique-string-or-guid>

**Example Request**

```bash
curl -X POST "http://localhost:8080/api/receipts" \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: 0f0f3f74-7ef2-4c9f-9b7b-0f5a0a0a0001" \
  -d '{
    "title": "Lunch",
    "amount": 1200,
    "currency": "JPY"
  }'
```

> **Note:** Sending the same request with the **same `Idempotency-Key`** will return the original response without creating a duplicate row in the database.

### Get Receipt By Id

```bash
curl "http://localhost:8080/api/receipts/{id}"
```

## Notes

- Credentials and connection strings in this repo are intended for local development only.
- In real systems, secrets should be injected via environment variables or secret managers.

## License

This project is licensed under the MIT License.