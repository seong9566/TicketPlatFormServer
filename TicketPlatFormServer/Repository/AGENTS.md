# PROJECT KNOWLEDGE BASE (Repository Layer)

## OVERVIEW
Repository layer for DB access in TicketHub backend.
Uses hybrid EF Core + Dapper and must remain data-access only.

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Core repository rules | `README.md` | Connection safety and transaction sharing |
| Read models | `ReadModels/` | Dapper projection targets |
| Domain repositories | subfolders (`Chat/`, `Sell/`, `Payment/`, etc.) | Interface + implementation pairs |

## CONVENTIONS
- Accept and return entities/primitives only; no DTO contracts here.
- Use EF Core for simple CRUD/relationship loading.
- Use Dapper for multi-join, aggregate, and high-volume read queries.
- Keep SQL constants close to repository modules when Dapper query grows.

## ANTI-PATTERNS
- Never execute repository calls in parallel in the same request scope (`Task.WhenAll` forbidden).
- Never manage transactions in repository code (service layer owns transaction lifecycle).
- Never open/close/dispose DI-provided scoped `IDbConnection` manually.
- Never add business validation/state rules in this layer.

## TRANSACTION RULE
- Service starts EF transaction.
- Dapper uses shared transaction via `efTransaction.GetDbTransaction()`.
- Keep EF and Dapper writes in one service-level unit of work.

## COMMANDS
```bash
# from TicketPlatFormServer/
dotnet build --project TicketPlatFormServer.sln

# optional scaffolding when DB schema changes
dotnet ef dbcontext scaffold "<connection-string>" Pomelo.EntityFrameworkCore.MySql \
  --output-dir DBModel --context-dir Repository --context TicketContext
```

## NOTES
- Primary risk in this layer is connection concurrency misuse.
- If adding new repository modules, follow existing folder naming by domain.
