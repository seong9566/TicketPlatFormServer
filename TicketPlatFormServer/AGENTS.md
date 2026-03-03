# PROJECT KNOWLEDGE BASE (API Project)

## OVERVIEW
ASP.NET Core 9 API project for TicketHub. Composition root, controllers, services, repositories, DB models, and realtime hub all live here.

## STRUCTURE
```text
TicketPlatFormServer/TicketPlatFormServer/
├── Program.cs              # bootstrap: DI, JWT auth, SignalR, hosted services, Polly policies
├── Controllers/            # 19 thin HTTP controllers (see Controllers/AGENTS.md)
├── Services/               # 25 domain service dirs + BackgroundServices + Common (see Services/AGENTS.md)
├── Repository/             # 19 repository dirs + TicketContext + ReadModels (see Repository/AGENTS.md)
├── DBModel/                # 70 EF-scaffolded entity POCOs (see DBModel/AGENTS.md)
├── DTO/                    # ~112 request/response DTOs, 21 domain subdirs (see DTO/AGENTS.md)
├── Hubs/ChatHub.cs         # SignalR hub /hubs/chat; groups user_{id}, room_{id}
├── Common/                 # AppException, GlobalExceptionMiddleware, ClaimsExtensions, EnvFileLoader, NicknameGenerator
├── Config/                 # 8 settings classes: JwtSettings, TossPaymentsSettings, FcmSettings, ChatSettings, EmailSettings, ResilienceSettings, SupabaseStorageSettings, StorageProviderSettings
├── Enum/                   # UserRoleEnum, UserRegisterProviderEnum, MessageType, WithdrawalStatusCode
├── api_spec/               # Markdown API specs (reference only, not runtime)
└── database_history/       # SQL dumps + db_restore.sh
```

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Startup/DI/middleware | `Program.cs` | Auth, SignalR, all service registrations, Polly resilience |
| HTTP routes | `Controllers/` + `Controllers/AGENTS.md` | Thin endpoint layer |
| Business flow | `Services/` + `Services/AGENTS.md` | AppException + orchestration |
| Data access | `Repository/` + `Repository/AGENTS.md` | EF Core + Dapper policy |
| DB entities | `DBModel/` + `DBModel/AGENTS.md` | EF-scaffolded POCOs |
| DTO contracts | `DTO/` + `DTO/AGENTS.md` | Request/response shapes |
| Realtime messaging | `Hubs/ChatHub.cs` | SignalR endpoint `/hubs/chat` |
| Error handling | `Common/Exception/` | AppException + GlobalExceptionMiddleware |
| JWT claims helpers | `Common/ClaimsExtensions.cs` | GetUserId, GetEmail, GetRole, GetProvider |

## CONVENTIONS
- Strict layer flow: Controller → Service → Repository → DB. No shortcuts.
- DTOs stay at controller/service boundary; DBModel entities stay inside repository boundary.
- XML docs and many comments are Korean; preserve language style when editing.
- `Program.cs` is the only startup entry point — no `Startup.cs` pattern.
- C# 12 primary constructors throughout: `public class FooService(IFooRepository repo)`.
- All async methods carry `*Async` suffix.

## ANTI-PATTERNS
- Never put business logic in controllers or repositories.
- Never call repositories in parallel in one request (`Task.WhenAll` = scoped connection conflict).
- Never bypass service layer for transactions or exception handling.
- Never leak DB entities to controllers as response types.
- Never manually parse JWT claims — use `ClaimsExtensions`.

## COMMANDS
```bash
dotnet restore --project TicketPlatFormServer.sln
dotnet build --project TicketPlatFormServer.sln
dotnet run --project TicketPlatFormServer/TicketPlatFormServer.csproj
dotnet test --project TicketPlatFormServer.Tests/TicketPlatFormServer.Tests.csproj
```

## NOTES
- Child AGENTS in `Repository/`, `Services/`, `Controllers/`, `DBModel/`, `DTO/` have layer-specific rules — prefer them.
- Sibling `TicketPlatFormServer.Tests/` has E2E test project with own `AGENTS.md`.
- API specs are markdown files in `api_spec/` (reference only).
- `Hubs/ChatHub.cs` uses `[Authorize]`; SignalR token passed via query string `?access_token=...`.
- Background services (5): ChatCleanupService, TransactionReservationCleanupService, TransactionAutoConfirmService, SettlementProcessingService, WithdrawalProcessingService — registered as `IHostedService`.
- Config settings bound from `appsettings.json`: `JwtSettings`, `TossPaymentsSettings`, `FcmSettings`, `ChatSettings`, `EmailSettings`, `ResilienceSettings`, `SupabaseStorageSettings`, `StorageProviderSettings`.
