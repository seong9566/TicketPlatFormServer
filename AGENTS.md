# PROJECT KNOWLEDGE BASE (Backend Workspace)

## OVERVIEW
Backend workspace root for TicketHub.
Contains the .NET solution and one API project folder with deeper layer-specific AGENTS files.

## STRUCTURE
```text
TicketPlatFormServer/
├── TicketPlatFormServer.sln
├── global.json
├── AGENTS.md
```text
TicketPlatFormServer/
├── TicketPlatFormServer.sln
├── global.json
├── AGENTS.md
└── TicketPlatFormServer/
    ├── Program.cs
    ├── Controllers/   # AGENTS.md
    ├── Services/      # AGENTS.md + README.md
    ├── Repository/    # AGENTS.md + README.md
    ├── DBModel/       # AGENTS.md
    ├── DTO/           # AGENTS.md
    └── database_history/
```

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| App bootstrap | `TicketPlatFormServer/Program.cs` | DI, middleware, auth, SignalR, hosted jobs, Polly policies |
| Controllers | `TicketPlatFormServer/Controllers/AGENTS.md` | Thin endpoint rules |
| Data access rules | `TicketPlatFormServer/Repository/AGENTS.md` | IDbConnection/transaction boundaries |
| Service rules | `TicketPlatFormServer/Services/AGENTS.md` | AppException and business flow |
| DTO contracts | `TicketPlatFormServer/DTO/AGENTS.md` | Request/response shapes, naming |
| DB entities | `TicketPlatFormServer/DBModel/AGENTS.md` | EF-scaffolded POCOs, re-scaffold rules |
| DB restore/dumps | `TicketPlatFormServer/database_history/` | SQL-first migration history |

## CONVENTIONS
- Solution root is one level above API project (`TicketPlatFormServer/TicketPlatFormServer.sln`).
- Target framework: .NET 9 (`global.json`, `net9.0`, nullable enabled).
- API keeps strict layered flow: Controller → Service → Repository → DB.
- C# 12 primary constructors throughout all service and repository classes.

## ANTI-PATTERNS
- Never call multiple repositories in parallel in one request when sharing scoped `IDbConnection`.
- Never pass DTOs into repository contracts (entities/primitives only at that boundary).
- Never add business logic inside repositories (service layer enforces rules).
- Never commit secrets in `appsettings.json`, `.env`, or any version-controlled config file.

## COMMANDS
```bash
dotnet restore --project TicketPlatFormServer.sln
dotnet build --project TicketPlatFormServer.sln
dotnet run --project TicketPlatFormServer/TicketPlatFormServer.csproj
dotnet test   # currently no backend test project in solution
```

## NOTES
- No CI workflow YAML exists; validation is local/manual.
- No test project in solution yet; service-layer unit tests are the recommended first target.
- For deeper rules, always prefer child AGENTS in `Controllers/`, `Services/`, `Repository/`, `DBModel/`, `DTO/`.
