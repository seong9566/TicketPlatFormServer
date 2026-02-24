# PROJECT KNOWLEDGE BASE (Service Layer)

## OVERVIEW
Service layer is the business boundary for TicketHub backend.
It orchestrates repositories, enforces rules, and converts exceptions to `AppException`.

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Exception pattern details | `README.md` | InnerException usage and examples |
| Chat workflow complexity | `Chat/ChatService.cs` | largest service, realtime + transaction coupling |
| Payment workflow complexity | `Payment/PaymentService.cs` | idempotency, external API integration |
| Sell workflow complexity | `Sell/SellService.cs` | ticket registration and validations |

## CONVENTIONS
- Validate request/business invariants in services, not controllers/repositories.
- Throw `AppException` for domain/business failures with proper HTTP status.
- Wrap DB/external API failures with `AppException(..., innerException)`.
- Keep controller actions thin; push orchestration to service methods.

## ANTI-PATTERNS
- Never perform direct SQL/DbContext transaction orchestration in controllers.
- Never call repositories in parallel under scoped connection lifecycle.
- Never leak DB entities to controller response directly without DTO mapping.
- Never swallow exceptions; preserve root cause when converting to `AppException`.

## QUALITY HOTSPOTS
- `ChatService.cs` and `PaymentService.cs` are high-change/high-risk; edit surgically.
- Preserve existing method naming and async suffix conventions (`*Async`).
- Reuse existing status-code and code-table lookup patterns before introducing new ones.

## COMMANDS
```bash
# from TicketPlatFormServer/
dotnet build --project TicketPlatFormServer.sln
dotnet run --project TicketPlatFormServer/TicketPlatFormServer.csproj
```

## NOTES
- Backend currently has no dedicated test project in solution.
- If adding tests later, prioritize service-level unit tests first.
