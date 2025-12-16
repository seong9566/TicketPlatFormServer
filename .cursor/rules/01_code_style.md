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
- Use meaningful XML comments for interfaces, methods (especially in repositories/services).
- Document public endpoints for Swagger.

## Best Practices
- **Early Returns**: Use early returns to simplify logic.
- **Dependency Injection**: Always use DI for service/repo instantiation.
- **Controller Logic**: Keep controllers thin; delegate to services.
- **Validation**: Validate input at controller level or model binding. Sanitize inputs.
- **Leaking Details**: Avoid leaking implementation details (e.g., EF entities) across layers.

## Cursor-Specific Hints
- Use `async`/`await` for all DB operations.
- Inject `TicketContext` via constructor.
- Include appropriate `using` statements.
- Follow the provided folder/layer structure strictly.
- Use `AppException` for business errors.
