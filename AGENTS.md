# AGENTS.md - TicketHub Development Guidelines

## Project Overview
**TicketHub**: 중고 티켓 거래 플랫폼 (티켓 가격을 원가 이하로만 판매)
- **Tech Stack**: ASP.NET Core 9, MySQL, EF Core 9, Dapper, JWT, SignalR, Supabase Storage
- **Framework**: .NET 9.0 with Nullable enabled, ImplicitUsings enabled

## Build, Test, and Development Commands

### Building and Running
```bash
# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build

# Run the API (Swagger UI: http://localhost:5224/swagger)
dotnet run --project TicketPlatFormServer

# Setup local environment (macOS)
scripts/setup.sh
```

### Testing
```bash
# No test project exists yet. To add tests:
# 1. Create TicketPlatFormServer.Tests project
# 2. Use xUnit for test framework
# 3. Use Moq for mocking dependencies
dotnet test

# Run single test (once tests exist)
dotnet test --filter "FullyQualifiedName~YourTestClassName.YourTestMethodName"
```

### Database
```bash
# Restore MySQL dump
TicketPlatFormServer/database_history/db_restore.sh   # macOS/Linux
TicketPlatFormServer/database_history/db_restore.bat  # Windows

# EF Core scaffolding (when schema changes)
dotnet ef dbcontext scaffold "YourConnectionString" Pomelo.EntityFrameworkCore.MySql \
  --output-dir DBModel --context-dir Repository --context TicketContext
```

## Project Architecture

### Layered Architecture (Strict Separation)
```
Controllers (HTTP) → Services (Business Logic) → Repositories (Data Access) → Database
     ↓                      ↓                           ↓
   DTOs  ←→  DTOs + Entities conversion  ←→  Entities only
```

### Directory Structure
- `Controllers/` - HTTP endpoints, thin layer delegating to services
- `Services/` - Business logic, validation, DTO ↔ Entity conversion
- `Repository/` - Data access via EF Core or Dapper
- `DTO/` - API request/response contracts (used only at Controller/Service boundary)
- `DBModel/` - EF entities (used only at Repository/DB boundary)
- `Common/` - Middleware, exceptions (AppException), shared utilities
- `Config/` - Configuration options bindings
- `Enum/` - Enumerations
- `Hubs/` - SignalR hubs for real-time features
- `api_spec/` - API specifications

**CRITICAL**: Never mix layers. DTOs never reach repositories; entities never reach controllers.

## Code Style and Formatting

### Naming Conventions
- **Classes/Methods/Properties**: `PascalCase` (e.g., `UserService`, `GetUserById`)
- **Variables/Parameters**: `camelCase` (e.g., `userRepository`, `userId`)
- **Private Fields**: `_camelCase` (e.g., `_userService`, `_db`)
- **Interfaces**: Prefix with `I` (e.g., `IUserRepository`, `IUserService`)

### Indentation and Braces
- **4 spaces** for indentation (not tabs)
- **Allman style**: braces on new lines
```csharp
public async Task<User> GetUserById(int userId)
{
    if (userId <= 0)
    {
        throw new AppException("Invalid user ID", HttpStatusCode.BadRequest);
    }
    return await _repo.GetUserByIdAsync(userId);
}
```

### Using Statements
- Place `using` statements at the top of the file
- ImplicitUsings is enabled, so common namespaces are auto-imported

### Async/Await
- **Always** use `async`/`await` for database operations
- Suffix async methods with `Async` (e.g., `GetUserByIdAsync`)

### XML Documentation
- Document all public interfaces and methods
- Use XML comments for Swagger documentation
```csharp
/// <summary>
/// 사용자 ID로 사용자 정보를 조회합니다.
/// </summary>
/// <param name="userId">사용자 ID</param>
/// <returns>사용자 정보</returns>
public async Task<User?> GetUserByIdAsync(int userId);
```

## Error Handling

### AppException Usage
- **Business rule violations**: Throw `AppException` in services
- **Simple validation**: No inner exception needed
```csharp
if (user == null)
{
    throw new AppException("User not found", HttpStatusCode.NotFound);
}
```

- **Database/External errors**: Include inner exception for debugging
```csharp
try
{
    await _db.SaveChangesAsync();
}
catch (DbUpdateException ex)
{
    throw new AppException("Failed to save user", HttpStatusCode.InternalServerError, ex);
}
```

### GlobalExceptionMiddleware
- Handles all unhandled exceptions
- Returns standardized `ApiResponse<T>` format
- Logs exceptions with context (never log passwords, tokens, PII)

## Data Access Patterns

### CRITICAL: Scoped IDbConnection Rule
⚠️ **IDbConnection is scoped (one per request)**. NEVER call repositories in parallel:
```csharp
// ❌ FORBIDDEN - concurrent access to same connection
await Task.WhenAll(
    _eventRepo.GetEventsByCategoryId(1),
    _ticketRepo.GetTicketsByEventId(100)
);

// ✅ CORRECT - sequential calls
var events = await _eventRepo.GetEventsByCategoryId(1);
var tickets = await _ticketRepo.GetTicketsByEventId(100);
```

### EF Core vs Dapper Selection
| Use Case | Tool | Reason |
|----------|------|--------|
| Simple CRUD | EF Core | Type safety, change tracking |
| Complex joins (3+ tables) | Dapper | SQL control, performance |
| Aggregations (SUM, COUNT, GROUP BY) | Dapper | Concise SQL |
| Large data reads | Dapper | Memory efficiency |
| Relationship loading (Include) | EF Core | Auto-mapping |

### Transaction Management
- Manage transactions in **Service layer**, never in repositories
- Share EF transaction with Dapper using `efTransaction.GetDbTransaction()`
- See `TicketPlatFormServer/Repository/README.md` for detailed transaction patterns

### Repository Rules
- **Return**: Entities or primitives (never DTOs)
- **Accept**: Entities or primitives (never DTOs)
- **No business logic**: Only data access
- **No transaction management**: Services handle transactions
- **No connection management**: DI container handles lifecycle

## Testing Strategy

### Unit Tests (Service Layer)
- Framework: xUnit
- Mocking: Moq for repository dependencies
- Focus: Business logic, validation, DTO ↔ Entity conversion

### Integration Tests (Repository Layer)
- Use actual database or Testcontainers
- Avoid mocking DbContext
- Test real SQL/EF behavior

## Commit Guidelines
Follow conventional commits:
- `feat: add user authentication endpoint`
- `fix: resolve null reference in payment service`
- `chore: update EF Core to 9.0.11`
- `docs: update API specs for ticket endpoints`
- `refactor: extract payment validation logic`

## Pull Request Checklist
- [ ] Clear description of changes
- [ ] Testing approach documented
- [ ] Database/migration impact noted
- [ ] API specs updated in `TicketPlatFormServer/api_spec/` (if endpoints changed)
- [ ] No credentials committed
- [ ] Follows existing code patterns

## Configuration
- `appsettings.json` - Base settings
- `appsettings.Development.json` - Dev overrides
- `appsettings.SupabaseStorage.json` - Storage configuration
- **Never commit secrets** - document required config in PR

## Key Dependencies
- **EF Core 9.0** (Pomelo.EntityFrameworkCore.MySql)
- **Dapper 2.1.66** - Raw SQL queries
- **BCrypt.Net-Next 4.0.3** - Password hashing
- **JWT Bearer** - Authentication
- **Swashbuckle 6.9.0** - Swagger/OpenAPI
- **Polly 8.5.0** - Resilience and transient fault handling
- **AWSSDK.S3** - AWS S3 integration (Supabase Storage)

## References
- Repository guide: `TicketPlatFormServer/Repository/README.md`
- Service guide: `TicketPlatFormServer/Services/README.md`
- Cursor rules: `.cursor/rules/*.md`
