---
trigger: always_on
---

# Data Layer (Repositories & Database)

## Entity Framework Core
- **Version**: Use EF Core 9.0.
- **Context**: Use `DbContext` (TicketContext) with constructor injection.
- **Scaffolding**:
  - Use Pomelo for MySQL.
  - Command: `dotnet ef dbcontext scaffold ... --output-dir DBModel --context-dir Repository`.
- **Entities**: ONLY entities (`User`, etc.) are allowed in repositories. NEVER DTOs.
- **Responsibility**: Repositories must remain thin and focused on persistence.

## Dapper
- **Usage**: Use for raw SQL when performance is critical or EF is insufficient.
- **Encapsulation**: Keep Dapper code inside Repository classes.
- **Mixing**: Avoid mixing Dapper and EF logic in the same method.
- **Preference**: Prefer EF Core unless there is a specific reason not to.

## Repository Rules
- **Return Types**: Return Entities, not DTOs.
- **Input Types**: Accept Entities or primitive types, not DTOs.
