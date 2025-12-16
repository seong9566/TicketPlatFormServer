---
trigger: always_on
---

# Code Style and Standards

## Naming Conventions
- **Classes/Methods**: PascalCase (e.g., `UserService`, `GetUserById`).
- **Variables/Parameters**: camelCase (e.g., `userRepository`, `userId`).
- **Interfaces**: Prefix with 'I' (e.g., `IUserRepository`, `IUserService`).

## File Structure
Group related files into these directories:
- `Controllers`
- `Services`
- `Repository`
- `DTO`
- `DBModel`
- `Common`
- `Enum`

## Documentation
- Use meaningful XML comments (`///`) for:
  - All public methods in services and repositories
  - All interface definitions
  - All controller actions
- Document public API endpoints to support Swagger documentation.
- Comments should describe **intent**, **parameters**, and **return values**.
- Avoid repeating the method name in the comment — focus on purpose.

## Comments and Annotations for Junior Developers
- All generated code should include clear and concise inline comments, especially for logic that may be non-obvious to junior .NET developers.
- Comments should:
  - Explain why the code exists (not just what it does).
  - Highlight important .NET concepts in use (e.g., DI, async/await, LINQ).
  - Be written in simple, beginner-friendly language.
- When performing validation, conversions (DTO ↔ Entity), or exception throwing, explain the reason in a short comment.
- For asynchronous code, note why `await` is used and what it does.
- When injecting dependencies (e.g., `TicketContext`, `IUserRepository`), briefly mention that it uses dependency injection.

## Best Practices
- **Early Returns**: Use early returns to simplify logic and reduce nesting.
- **Dependency Injection**: Always use DI for injecting services and repositories.
- **Controller Logic**: Keep controllers minimal. Delegate business logic to services.
- **Validation**: Validate and sanitize inputs at the controller or model binding level.
- **Encapsulation**: Do not expose EF Core entities directly to controllers or clients.
- **Layered Responsibility**:
  - **DTO**: For communication with clients
  - **Entity**: For DB access only
  - **Repository**: Only accepts and returns entities
  - **Service**: Converts DTO ↔ Entity, handles business rules

## Cursor-Specific Hints
- Use `async`/`await` for all DB operations and API methods.
- Inject `TicketContext` using constructor injection.
- Include necessary `using` directives at the top of each file.
- Use `AppException` for domain-specific or business rule violations.
- Adhere strictly to the provided layered file/folder structure.
- When creating methods, follow this order in file:
  1. Public methods
  2. Private helper methods