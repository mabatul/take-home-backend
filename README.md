# take-home-backend

The backend for a loan application system. It receives loan applications,
evaluates them against a set of business rules, persists customers and
applications, and notifies an external service — returning an approval or
denial decision.

## Tech Stack

- [.NET](https://dotnet.microsoft.com) 10 (C#)
- [ASP.NET Core](https://learn.microsoft.com/aspnet/core) Web API
- [Entity Framework Core](https://learn.microsoft.com/ef/core) with SQLite
- [xUnit](https://xunit.net) for unit and integration tests
- Swagger / OpenAPI for API documentation

## Features

- `POST /api/Loan` endpoint that submits a loan application and returns an
  approval or denial.
- Pluggable **rule engine** that evaluates an application against business
  rules:
  - `DeniedStatesRule` — denies applications from configured states.
  - `SsnBlacklistRule` — denies applications whose SSN is blacklisted.
- Distinguishes new vs. **returning customers** by SSN and updates their
  existing records accordingly.
- Persists customers and applications to SQLite via EF Core (auto-migrate on
  startup), wrapped in a transaction.
- Publishes application events asynchronously to an external service
  (`ApplicationEventProcessor` background service).

## Project Structure

```
LoanApplication.slnx
src/
  LoanApplication.Api/            # ASP.NET Core API, controllers, config
  LoanApplication.Core/           # Domain models, DTOs, interfaces, rules
  LoanApplication.Infrastructure/ # EF Core persistence, repositories, events, external service
tests/
  LoanApplication.Tests/          # Unit and integration tests
```

## Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) 10 or later

### Run the API

```bash
dotnet run --project src/LoanApplication.Api
```

The API listens on the ports defined in `launchSettings.json` (by default
`http://localhost:5175`). Swagger UI is available in development.

### Run the tests

```bash
dotnet test
```

## Configuration

Settings live in `src/LoanApplication.Api/appsettings.json` (override with
`appsettings.Development.json` or user secrets):

| Setting                      | Description                                            |
| ---------------------------- | ------------------------------------------------------ |
| `ConnectionStrings:DefaultConnection` | SQLite connection string (defaults to `LoanApplication.db`) |
| `DeniedStates`               | State codes that are denied (e.g. `["NY"]`)            |
| `BlacklistedSsns`            | SSNs that are denied (test data)                       |
| `ExternalService:BaseUrl`    | Base URL of the external mock service (defaults to `http://localhost:3001`) |

## External Mock Service

The API calls an external service to record customers and applications. In
this take-home project that is the `mock-service` (Node/Express) exposing
`/api/customers` and `/api/applications`. Configure its URL via
`ExternalService:BaseUrl`.
