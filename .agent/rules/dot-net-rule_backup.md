---
trigger: always_on
---

You are an expert in .NET 9, ASP.NET Core MVC, REST API design, and layered backend architecture using EF Core and Dapper.

Project Architecture
- This project uses a layered architecture:
  - Controllers handle HTTP requests and responses.
  - Services handle business logic and convert DTOs to entities.
  - Repositories directly interact with the database via EF Core or Dapper.
  - DTOs are used only at the controller/service layer boundary.
  - Entities are used only in the repository layer and below.

Code Style and Structure
- Follow C# naming conventions (PascalCase for classes and methods, camelCase for variables and parameters).
- Group related files into directories: `Controllers`, `Services`, `Repository`, `DTO`, `DBModel`, `Common`, `Enum`.
- Interfaces should be prefixed with 'I' (e.g., `IUserRepository`, `IUserService`).
- Use meaningful XML comments for interfaces and methods, especially in repositories and services.
- Use early returns in methods to simplify branching logic.
- Favor dependency injection for service and repository instantiation.

Entity Framework Core
- Use EF Core 9.0 for most data access. Use `DbContext` (TicketContext) with constructor injection.
- Scaffold DBModel classes using Pomelo for MySQL:
  - Use `dotnet ef dbcontext scaffold` with `--output-dir DBModel --context-dir Repository`.
- Repositories must never accept or return DTOs — only entities (`User`, etc.).
- Repositories should remain thin and focused only on persistence concerns.

Dapper
- Use Dapper for raw SQL queries in cases where performance is critical or EF Core is insufficient.
- Keep Dapper usage encapsulated in repository classes. Prefer EF Core unless specific reasons dictate otherwise.
- Avoid mixing Dapper and EF logic in the same method.

Services
- Service layer handles DTO to Entity conversion and orchestrates logic across repositories.
- Services may validate enums, encrypt passwords (e.g., with BCrypt), and throw `AppException` for business rule violations.
- Never perform database operations directly in services; always delegate to repository methods.

DTO Handling
- Create distinct DTO classes for request and response (e.g., `RegisterUserReqDto`, `RegisterUserRespDto`).
- Never expose internal entity structures (like EF entities) directly to the client.
- DTO validation should occur at the controller level or via model binding.

Error Handling
- Use custom exception classes like `AppException` to throw domain-specific errors with status codes and messages.
- Catch and translate unhandled exceptions into appropriate HTTP responses in middleware or exception filters.

Logging
- Use structured logging (e.g., with `ILogger<T>`) in services and controllers.
- Log all exceptions with context.
- Avoid logging sensitive data like passwords or tokens.

Swagger (Swashbuckle)
- Ensure all public endpoints are documented with XML comments for Swagger.
- Annotate DTOs with attributes if needed for better Swagger rendering.

Security
- Hash passwords using BCrypt (via BCrypt.Net-Next).
- Validate inputs (especially enums and strings) rigorously.
- Do not store plain text passwords.

Best Practices
- Keep controller methods thin — delegate logic to services.
- Always validate and sanitize input, especially when binding from request bodies.
- Avoid leaking implementation details across layers.
- Structure the application to support future Docker containerization and cloud deployment.

Testing
- Write unit tests for services using mocking for repository dependencies.
- Repositories should be tested using integration tests with an actual database.
- Prefer `xUnit` and `Moq` (or equivalent) for test projects.

Cursor-Specific Hints
- When generating code:
  - Use `async`/`await` for all database operations.
  - Always inject `TicketContext` via constructor.
  - Include appropriate `using` statements.
  - Use DTOs for input/output at the controller level.
  - Follow the provided folder and layer structure strictly.
  - Use `AppException` for business errors.