# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

**TicketHub** — a secondary ticket marketplace where resale prices are capped at face value. ASP.NET Core 9 backend serving a Flutter mobile app.

Tech stack: ASP.NET Core 9, MySQL 9, EF Core 9 (Pomelo), Dapper, JWT Bearer, SignalR, Supabase Storage, Toss Payments PG, Polly, Firebase Cloud Messaging.

## Commands

```bash
# Run from solution root
dotnet restore --project TicketPlatFormServer.sln
dotnet build  --project TicketPlatFormServer.sln
dotnet run    --project TicketPlatFormServer/TicketPlatFormServer.csproj

# Re-scaffold EF models from DB (run from TicketPlatFormServer/ subfolder)
dotnet ef dbcontext scaffold "<connection-string>" Pomelo.EntityFrameworkCore.MySql \
  --output-dir DBModel --context TicketContext --context-dir Repository --force

# DB restore
mysql -u root -p TicketPlatFormDB < TicketPlatFormServer/database_history/TicketPlatFormDB_dump.sql
```

Dev server: `http://localhost:5224` · Swagger UI: `http://localhost:5224/swagger`

No automated test project exists. No CI pipeline.

## Architecture

Strict layered flow — never skip layers:

```
Controller  →  Service  →  Repository  →  DB (EF Core / Dapper)
                  ↓
            SignalR Hub  →  Client
                  ↓
          Supabase Storage (file uploads)
```

### Directory map

| Path | Purpose |
|------|---------|
| `Program.cs` | Single composition root; DI, middleware, auth, SignalR, hosted services |
| `Controllers/` | Thin HTTP endpoints — no business logic |
| `Services/` | Business orchestration; owns transactions and exception conversion |
| `Repository/` | Data access only (EF Core + Dapper); no business logic |
| `DBModel/` | EF Core entity classes (scaffolded from DB) |
| `DTO/` | Request/response objects; stay at controller/service boundary only |
| `Hubs/ChatHub.cs` | SignalR endpoint at `/hubs/chat` |
| `Common/Exception/` | `AppException`, `GlobalExceptionMiddleware` |
| `Config/` | Strongly-typed settings classes bound in `Program.cs` |
| `Services/BackgroundServices/` | `ChatCleanupService`, `TransactionReservationCleanupService`, `TransactionAutoConfirmService`, `SettlementProcessingService` |
| `database_history/` | SQL dump history and migration scripts |
| `api_spec/` | Markdown API specifications |

### Configuration loading order (Program.cs)

1. `.env` and `db_connect.env` (loaded via `EnvFileLoader`)
2. `appsettings.json` → `appsettings.{Environment}.json`
3. `appsettings.SupabaseStorage.json`
4. `appsettings.TossPayments.json`
5. Environment variables

### Key modules

- **Auth**: JWT Bearer; SignalR receives token via `?access_token=` query string
- **Payment**: Toss Payments integration in `Services/Payment/TossPaymentsService.cs`; idempotency is critical — edit surgically
- **Escrow flow**: `HOLD → RELEASED → (FROZEN | REFUNDED)`; settlement runs D+3 via background service
- **Chat**: 1:1 buyer–seller rooms, realtime via SignalR, image uploads to Supabase Storage
- **Storage**: Single Supabase bucket `chat-images`; paths: `chat/{roomId}/`, `profiles/{userId}/`, `tickets/{ticketId}/`; always use Signed URLs with `/storage/v1` prefix
- **Resilience**: Polly retry + circuit breaker applied to `SupabaseStorageUploader` and Toss Payments `HttpClient`

## Critical rules

### IDbConnection concurrency — DO NOT violate

`IDbConnection` is registered as **Scoped** (one per request). Never call two repositories concurrently inside one request:

```csharp
// ❌ FORBIDDEN — concurrent access to shared connection
await Task.WhenAll(repoA.GetAsync(), repoB.GetAsync());

// ✅ REQUIRED — sequential calls
var a = await repoA.GetAsync();
var b = await repoB.GetAsync();
```

### Transactions — service layer owns them

```csharp
using var efTx = await _db.Database.BeginTransactionAsync();
// EF Core writes → await _db.SaveChangesAsync()
// Dapper writes  → pass efTx.GetDbTransaction() as transaction parameter
await efTx.CommitAsync();
```

Repositories never call `BeginTransaction`, `Commit`, `Rollback`, or `Dispose` on connections.

### Exception handling

All business failures throw `AppException`; the `GlobalExceptionMiddleware` converts it to HTTP responses automatically.

```csharp
// Simple validation
throw new AppException("이벤트를 찾을 수 없습니다.", HttpStatusCode.NotFound);

// Wrapping external/DB failures — always preserve InnerException
throw new AppException("결제 처리 오류.", HttpStatusCode.BadGateway, ex);

// Re-throw AppException as-is inside catch-all blocks
catch (AppException) { throw; }
```

### Repository authoring

- Use **EF Core** for simple CRUD and relationship loading.
- Use **Dapper** for multi-join, aggregate, or high-volume read queries.
- Place Dapper SQL constants in a sibling `*Queries.cs` file (e.g., `PaymentQueries.cs`).
- Read models used by Dapper projections live in `Repository/ReadModels/`.
- DTOs must never appear in repository method signatures.

### Code style

- Comments and XML docs are in Korean — preserve the language when editing existing files.
- All async methods use the `*Async` suffix.
- High-risk files requiring surgical edits: `ChatService.cs`, `PaymentService.cs`, `SellService.cs`.
