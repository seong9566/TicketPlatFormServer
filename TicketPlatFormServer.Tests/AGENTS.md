# PROJECT KNOWLEDGE BASE (E2E Tests)

## OVERVIEW
xUnit integration test project for TicketHub backend. 11 test suites covering all major feature flows. Tests run against isolated MySQL test database with WireMock for external services.

## STRUCTURE
```text
TicketPlatFormServer.Tests/
├── TestWebApplicationFactory.cs   # In-process test server; DI overrides; WireMock init; test DB setup
├── TestDbManager.cs               # DB lifecycle: create TicketPlatFormDB_Test, apply dumps/migrations, cleanup
├── TestDataSeeder.cs              # Fixture creator: users with Guid-based emails, BCrypt passwords, profiles
├── appsettings.Testing.json       # Test environment config (test DB connection, JWT settings)
├── xunit.runner.json              # Disables parallel execution (sequential only)
├── Helpers/
│   └── TestAuthHelper.cs          # JWT token generation: GenerateUserToken(), GenerateAdminToken(), AddAuthHeader()
├── Mocks/
│   ├── WireMockSetup.cs           # HTTP stubs: Supabase Storage, Google/Kakao OAuth, FCM
│   └── NoOpEmailService.cs        # No-op SMTP to prevent real email sends
└── Tests/
    ├── AuthFlowTests.cs           # Register, login, token refresh, logout, social OAuth
    ├── PublicApiTests.cs           # Unauthenticated endpoints: categories, events, search, home
    ├── SellFlowTests.cs           # Ticket listing, categories, features, seller workflows
    ├── ChatFlowTests.cs           # SignalR chat, buyer/seller messaging, room management
    ├── PaymentFlowTests.cs        # Payment requests, webhook handling, order lookups
    ├── DisputeFlowTests.cs        # Dispute creation, evidence submission, resolution
    ├── ProfileTests.cs            # User profile updates, identity verification
    ├── BankAccountTests.cs        # Account verification, validation, management
    ├── BalanceTests.cs            # Escrow holds, balance queries, fund management
    ├── WithdrawalTests.cs         # Settlement, withdrawal requests, fund transfers
    └── AdminTests.cs              # Admin endpoints, permission checks, moderation
```

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Test server setup | `TestWebApplicationFactory.cs` | Extends `WebApplicationFactory<Program>`; DI overrides for mocks |
| DB init/cleanup | `TestDbManager.cs` | Creates test DB, applies SQL dumps, deletes `test_*@test.com` users |
| Auth in tests | `Helpers/TestAuthHelper.cs` | Same JWT secret/claims as production; injects Bearer header |
| External service mocks | `Mocks/WireMockSetup.cs` | Supabase, Google OAuth, Kakao OAuth, FCM stubs |

## CONVENTIONS
- Tests are **E2E integration**: full HTTP request/response cycle through controllers → services → repository → real DB.
- All test classes use `[Collection("Sequential")]` — no parallel execution (shared DB).
- Test classes implement `IClassFixture<TestWebApplicationFactory>` for factory reuse.
- Use `IAsyncLifetime` for setup/teardown (e.g., creating buyer/seller users in `InitializeAsync`).
- Test users have `test_{guid}@test.com` emails — `TestDbManager` cleans these on teardown.
- Assertions use FluentAssertions (`response.Should().Be200Ok()`).

## ANTI-PATTERNS
- Never run tests in parallel — single shared test DB connection.
- Never use production database — always `TicketPlatFormDB_Test`.
- Never mock internal services (controllers, services, repositories) — these are integration tests.
- Never skip `TestDbManager` cleanup — test data must not leak between runs.
- Never mock Toss Payments — uses real test API (test keys in `appsettings.Testing.json`).

## COMMANDS
```bash
# from TicketPlatFormServer/
dotnet test --project TicketPlatFormServer.Tests/TicketPlatFormServer.Tests.csproj

# with verbose output
dotnet test --project TicketPlatFormServer.Tests/TicketPlatFormServer.Tests.csproj --verbosity normal

# run specific test class
dotnet test --project TicketPlatFormServer.Tests/TicketPlatFormServer.Tests.csproj --filter "FullyQualifiedName~AuthFlowTests"
```

## NOTES
- Requires running MySQL with `TicketPlatFormDB_Test` database (auto-created by `TestDbManager`).
- `init_test_db.sh` available for manual test DB initialization.
- WireMock runs on random port per test run — `TestWebApplicationFactory` injects mock URLs into DI.
- `NoOpEmailService` replaces real SMTP service in DI to prevent email sends.
- Test project references the API project directly — shares all production types.
