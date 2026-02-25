# PROJECT KNOWLEDGE BASE (DBModel Layer)

## OVERVIEW
EF Core 9 entity classes scaffolded from MySQL schema. 66 files — largest code directory. Treat as read-only schema source; business logic never lives here.

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Schema source of truth | `TicketContext.cs` (in `Repository/`) | DbContext with all DbSet registrations |
| Entity relationships | Individual entity files | Navigation properties reflect FK structure |
| Re-scaffolding procedure | `Repository/AGENTS.md` → COMMANDS | Use `dotnet ef dbcontext scaffold` when schema changes |

## CONVENTIONS
- Entities are plain POCOs: only properties + navigation properties.
- Naming follows DB column names (snake_case → PascalCase via EF conventions).
- Nullable reference types enabled — `string?` = nullable column, `string` = NOT NULL.
- Collections on parent entities (`ICollection<T>`) = one-to-many; use `.Include()` in EF queries.

## ANTI-PATTERNS
- Never add methods, computed properties, or business logic to entity classes.
- Never reference DTOs or service types from this layer.
- Never manually edit scaffolded files unless adding EF fluent config — re-scaffold when schema drifts.
- Never use entities above the repository boundary (entities must not reach controllers/services as response types).

## NOTES
- Re-scaffold command: see `Repository/AGENTS.md` COMMANDS section.
- `TicketContext.cs` lives in `Repository/`, not here — that is intentional (context is a repository concern).
- After re-scaffolding, review navigation property changes carefully; EF may alter existing `Include` chains.
