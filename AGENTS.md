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
└── TicketPlatFormServer/
    ├── Program.cs
    ├── Repository/
    │   ├── AGENTS.md
    │   └── README.md
    ├── Services/
    │   ├── AGENTS.md
    │   └── README.md
    └── database_history/
```

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| App bootstrap | `TicketPlatFormServer/Program.cs` | DI, middleware, auth, SignalR, hosted jobs |
| Data access rules | `TicketPlatFormServer/Repository/AGENTS.md` | IDbConnection/transaction boundaries |
| Service rules | `TicketPlatFormServer/Services/AGENTS.md` | AppException and business flow |
| DB restore/dumps | `TicketPlatFormServer/database_history/` | SQL-first migration history |

## CONVENTIONS
- Solution root is one level above API project (`TicketPlatFormServer/TicketPlatFormServer.sln`).
- Target framework and toolchain: .NET 9 (`global.json`, `net9.0`, nullable enabled).
- API keeps strict layered flow: Controller -> Service -> Repository -> DB.

## ANTI-PATTERNS
- Never call multiple repositories in parallel in one request when sharing scoped `IDbConnection`.
- Never pass DTOs into repository contracts.
- Never add business logic inside repositories.
- Never commit secrets in appsettings or env files.

## COMMANDS
```bash
dotnet restore --project TicketPlatFormServer.sln
dotnet build --project TicketPlatFormServer.sln
dotnet run --project TicketPlatFormServer/TicketPlatFormServer.csproj
dotnet test   # currently no backend test project in solution
```

## NOTES
- No CI workflow YAML exists; validation is local/manual.
- For deeper rules, always prefer child AGENTS in `Repository/` and `Services/`.
