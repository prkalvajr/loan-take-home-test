# Loan Application — Fundo Take-Home

> Demo video: _coming soon_

A simplified loan application flow. A Next.js form posts to a .NET API. The API's rule engine approves or denies the application. Approved applications are saved to PostgreSQL together with an outbox message in one transaction. A background worker then delivers the message to a mock external service.

**[ARCHITECTURE.md](ARCHITECTURE.md)** explains the design: structure, the rule engine and how to add a rule, the outbox and background delivery, failure handling and trade-offs.

## Repository layout

```
Loans.sln                         Opens everything in Visual Studio (backend, mock, frontend)
docker-compose.yml                PostgreSQL + Api + external service mock + frontend
ARCHITECTURE.md                   Design, rule engine, outbox, failure handling, trade-offs
backend/src/
  Loans.Domain/                   Entities and value objects (no dependencies)
  Loans.Application/              Use case, rule engine, ports (depends on Domain)
  Loans.Infrastructure/           EF Core / PostgreSQL, outbox worker, HTTP client (depends on Application)
  Loans.Api/                      Minimal API endpoint, composition root
backend/tests/
  Loans.UnitTests/                Rule engine, domain, use case, outbox policy, HTTP client (no infrastructure)
  Loans.IntegrationTests/         Endpoint, transaction and outbox against real PostgreSQL (Testcontainers)
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
| PostgreSQL       | localhost:5434 (user, password and database: `loans`) |

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

```bash
dotnet test Loans.sln                              # everything: 69 unit + 13 integration tests
dotnet test backend/tests/Loans.UnitTests          # unit tests only, no Docker needed
dotnet test backend/tests/Loans.IntegrationTests   # integration tests only
```

- **Integration tests need Docker running.** Testcontainers starts a throwaway `postgres:17` container for the run and removes it afterwards; the `docker compose` database is not touched.
- What is covered:
  - **Rule engine:** each rule, all rules evaluated, both reasons returned together.
  - **Returning customer:** the same customer and application are updated and an `ApplicationUpdated` event is written (unit and integration); a denied returning customer changes nothing.
  - **Endpoint:** approved, denied (NY, blacklisted, both), returning customer, `400` validation errors, malformed body.
  - **Transaction:** when the outbox insert fails, the customer and application are rolled back too.
  - **Outbox:** delivery order, POST vs PUT, retries, permanent failures, one customer's failure not blocking others.

## Test data

Any valid values work. These trigger each path:

| Scenario | What to enter |
| --- | --- |
| **Approved** | Any state except NY and an SSN not in the list below, e.g. state `TX`, SSN `246-81-3579` |
| **Denied: state** | State `NY` → `STATE_NOT_ALLOWED` |
| **Denied: blacklisted SSN** | `000-00-0001`, `000-00-0002` or `666-12-3456` → `SSN_BLACKLISTED` |
| **Denied: both reasons** | State `NY` **and** a blacklisted SSN |
| **Returning customer** | Submit an approved application, then submit again with the **same SSN** and different details or amount. The page says "Your application was updated" and the application ID stays the same. |
| **External service received it** | Open http://localhost:5090/applications up to 10 seconds after an approval (the worker checks the outbox every 10 s). After an update the record shows `version: 2`. |
| **Retries** | Start with `FAIL_RATE=1 docker compose up --build` and watch `docker compose logs -f api`: attempts 1–5 roughly 10 s apart (the 2, 4, 8, 16 s retry waits are rounded up to the next 10 s worker tick), then `Failed` after about 50 s. |

A full example: Maria Lopez, Lopez Bakery LLC, 500 Congress Ave, Austin, TX 78701, SSN `246-81-3579`, amount `50000`.

The blacklisted SSNs are never-issued numbers, seeded into the `blacklisted_ssns` table by the initial migration. To add one while the stack is running:

```bash
docker compose exec db psql -U loans -d loans \
  -c "INSERT INTO blacklisted_ssns (ssn, reason, created_at) VALUES ('123456789', 'Manual', now());"
```

To see the outbox and the state of each message:

```bash
docker compose exec db psql -U loans -d loans \
  -c "SELECT id, type, status, attempts, next_attempt_at, last_error FROM outbox_messages ORDER BY id;"
```

To start again with an empty database: `docker compose down -v`.

## Changing the database schema

The EF Core configurations in `backend/src/Loans.Infrastructure/Persistence/Configurations/` are the source of truth. Migrations are generated from them and applied automatically when the API starts.

```bash
dotnet tool install --global dotnet-ef   # once
dotnet ef migrations add <Name> -p backend/src/Loans.Infrastructure -s backend/src/Loans.Api -o Persistence/Migrations
```

Review the generated migration and commit it together with the code change. Never edit a migration that has already been shared; add a new one instead.
