# Loan Application — Fundo Take-Home

> Demo video: _coming soon_

A simplified loan application flow. A Next.js form posts to a .NET API. The API's rule engine approves or denies the application. Approved applications are saved to PostgreSQL together with an outbox message in one transaction. A background worker then delivers the message to a mock external service.

> **Status: work in progress.** The skeleton, the frontend pages and the mock external service are in place. The domain model, rule engine, persistence, outbox worker and tests are next.

## Repository layout

```
Loans.sln                         Opens everything in Visual Studio (backend, mock, frontend)
docker-compose.yml                PostgreSQL + Api + external service mock + frontend
backend/src/
  Loans.Domain/                   Entities and value objects (no dependencies)
  Loans.Application/              Use case, rule engine, ports (depends on Domain)
  Loans.Infrastructure/           EF Core / PostgreSQL, outbox worker, HTTP client (depends on Application)
  Loans.Api/                      Minimal API endpoint, composition root
external-service/
  ExternalService.Mock/           Mock external service (.NET minimal API, in-memory)
frontend/                         Next.js app: form, /approved, /denied
```

## Prerequisites

- .NET SDK 10
- Node.js 20 or later
- Docker Desktop

## Running

### Everything with Docker

```bash
docker compose up --build
```

| Service          | URL                    |
| ---------------- | ---------------------- |
| Frontend         | http://localhost:3000  |
| Api              | http://localhost:5080  |
| External service | http://localhost:5090  |
| PostgreSQL       | localhost:5432 (user, password and database: `loans`) |

To make the external service fail so you can watch the retries: `FAIL_RATE=1 docker compose up --build`.

### Locally, for development

```bash
docker compose up db                                       # PostgreSQL only
dotnet run --project external-service/ExternalService.Mock # http://localhost:5090
dotnet run --project backend/src/Loans.Api                 # http://localhost:5080

cd frontend
npm install
npm run dev                                                # http://localhost:3000
```

The frontend calls the API at `NEXT_PUBLIC_API_URL` (default `http://localhost:5080`, see `frontend/.env.example`).

## External service contract

The mock stores applications in memory, keyed by **our** application ID. Data is lost on restart.

| Method | Path                            | Sent when              | Behaviour |
| ------ | ------------------------------- | ---------------------- | --------- |
| POST   | `/applications`                 | `ApplicationCreated`   | `201` on create. An ID it already has returns `200` and creates nothing. |
| PUT    | `/applications/{applicationId}` | `ApplicationUpdated`   | `200`, replaces the record. Unknown ID returns `404`. |
| GET    | `/applications`                 | Demo only              | Lists everything received, with `createdAt`, `updatedAt` and `version`. |

- **`Idempotency-Key` header**: the outbox message ID. A key that already succeeded is logged and ignored with `200`. This makes at-least-once delivery from the outbox safe.
- **`FAIL_RATE`** (0 to 1): share of POST/PUT requests that return `500`. `0` by default.
- **Payload**: only the last 4 SSN digits are shared.

```json
{
  "applicationId": "…",
  "customerId": "…",
  "requestedAmount": 25000.00,
  "customer": {
    "firstName": "Jane",
    "lastName": "Doe",
    "ssnLast4": "6789",
    "companyName": "Acme LLC",
    "address": { "street": "100 Main St", "city": "Austin", "state": "TX", "zipCode": "73301" }
  }
}
```

## Tests

_Coming with the backend logic._

## Test data

_Blacklisted SSNs and inputs for the approved, denied and returning-customer cases come with the rule engine._
