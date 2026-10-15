# Loan Application — Fundo Take-Home

> Demo video: _coming soon_

A Next.js form posts to a .NET API. A rule engine approves or denies the application. Approved applications are saved to PostgreSQL together with an outbox message in one transaction, and a background worker delivers that message to a mock external service.

The design (structure, rule engine, transactions, background delivery, trade-offs) is explained in **[ARCHITECTURE.md](ARCHITECTURE.md)**.

```
backend/src/       Loans.Domain · Loans.Application · Loans.Infrastructure · Loans.Api
backend/tests/     Loans.UnitTests · Loans.IntegrationTests
external-service/  ExternalService.Mock (.NET minimal API, in-memory)
frontend/          Next.js app: form, /approved, /denied
Loans.sln          Opens everything in Visual Studio
```

## Running

Prerequisites: .NET SDK 10, Node.js 20+, Docker Desktop.

**Everything with Docker:**

```bash
docker compose up --build
```

| Service | URL |
| --- | --- |
| Frontend | http://localhost:3000 |
| API | http://localhost:5080 |
| External service | http://localhost:5090/applications (lists what it received) |
| PostgreSQL | `localhost:5434`, user, password and database `loans` |

**Locally, for development:**

```bash
docker compose up db                                       # PostgreSQL only
dotnet run --project external-service/ExternalService.Mock # http://localhost:5090
dotnet run --project backend/src/Loans.Api                 # http://localhost:5080
cd frontend && npm install && npm run dev                  # http://localhost:3000
```

The API applies database migrations on startup. `docker compose down -v` deletes the database. The mock keeps data in memory, so restarting it empties it.

## Tests

```bash
dotnet test Loans.sln
```

This runs 69 unit tests and 13 integration tests. The integration tests need Docker: they start a throwaway PostgreSQL container with Testcontainers and run the real API against it. The tests cover:

- the rule engine
- the returning-customer path, including a denied returning customer changing nothing
- the endpoint (approved, denied, `400`)
- rollback when the outbox insert fails
- outbox delivery and retries

## Test data

| Scenario | Input |
| --- | --- |
| Approved | Any state except NY, an SSN not blacklisted. Example: Maria Lopez, Lopez Bakery LLC, 500 Congress Ave, Austin, TX 78701, SSN `246-81-3579`, amount `50000` |
| Denied: state | State `NY` → `STATE_NOT_ALLOWED` |
| Denied: blacklist | SSN `000-00-0001`, `000-00-0002` or `666-12-3456` → `SSN_BLACKLISTED` (seeded never-issued numbers) |
| Denied: both | State `NY` and a blacklisted SSN → both reasons |
| Returning customer | Submit again with the **same SSN** and a new amount or details: "Your application was updated", same application ID |
| External service | Within 10 s of an approval the record appears at http://localhost:5090/applications. After an update it shows `version: 2`. |
| Retries | `FAIL_RATE=1 docker compose up --build`, then `docker compose logs -f api`: 5 attempts about 10 s apart, then `Failed` |

`FAIL_RATE=1` is Bash syntax. In PowerShell, run `$env:FAIL_RATE=1; docker compose up --build`.

To watch the outbox:

```bash
docker compose exec db psql -U loans -d loans -c "SELECT id, type, status, attempts, last_error FROM outbox_messages ORDER BY id;"
```

## External service contract

| Request | Sent for | Response |
| --- | --- | --- |
| `POST /applications` | `ApplicationCreated` | `200`. An ID it already has also returns `200` and creates nothing. |
| `PUT /applications/{applicationId}` | `ApplicationUpdated` | `200`; unknown ID → `404` |

Contract choices:

- Records are keyed by **our** application ID, so we can address updates without storing an ID from the other side.
- Every request carries an `Idempotency-Key` header, set to the outbox message ID. Repeats are ignored, which makes at-least-once delivery safe.
- `FAIL_RATE` (0–1) makes that share of requests return `500`.
- Retries are described in [ARCHITECTURE.md](ARCHITECTURE.md#background-event-and-external-service).

The payload shares only the last 4 SSN digits:

```json
{
  "applicationId": "…", "customerId": "…", "requestedAmount": 25000.00,
  "customer": {
    "firstName": "Jane", "lastName": "Doe", "ssnLast4": "6789", "companyName": "Acme LLC",
    "address": { "street": "100 Main St", "city": "Austin", "state": "TX", "zipCode": "73301" }
  }
}
```
