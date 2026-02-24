# PROJECT KNOWLEDGE BASE (API Project)

## OVERVIEW
ASP.NET Core 9 API project for TicketHub.
Composition root, controllers, services, repositories, DB models, and realtime hub live here.

## STRUCTURE
```text
TicketPlatFormServer/TicketPlatFormServer/
├── Program.cs
├── Controllers/
├── Services/
│   └── AGENTS.md
├── Repository/
│   └── AGENTS.md
├── DBModel/
├── DTO/
├── Hubs/
├── Common/
├── Config/
└── database_history/
```

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Startup/DI/middleware | `Program.cs` | Auth, SignalR, DI, hosted services |
| HTTP routes | `Controllers/` | Thin endpoint layer |
| Business flow | `Services/` | AppException and orchestration |
| Data access details | `Repository/` | EF Core + Dapper policy |
| Realtime messaging | `Hubs/ChatHub.cs` | SignalR endpoint `/hubs/chat` |

## CONVENTIONS
- Keep layering strict: Controller -> Service -> Repository.
- DTOs stay at controller/service boundary; DBModel stays in repository boundary.
- XML docs and many comments are Korean; preserve language style when editing.
- `Program.cs` is the only startup entrypoint (no Startup.cs pattern).

## ANTI-PATTERNS
- Do not place business logic in controllers or repositories.
- Do not call repositories in parallel within one request scope.
- Do not bypass service layer for transaction or exception orchestration.

## COMMANDS
```bash
dotnet restore --project TicketPlatFormServer.sln
dotnet build --project TicketPlatFormServer.sln
dotnet run --project TicketPlatFormServer/TicketPlatFormServer.csproj
```

## NOTES
- Child AGENTS in `Repository/` and `Services/` override this file for layer-specific rules.
- API specs are markdown files in `api_spec/`.
