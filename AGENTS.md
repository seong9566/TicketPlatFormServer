# Repository Guidelines

## Project Structure & Module Organization
- `TicketPlatFormServer/` hosts the ASP.NET Core API.
- `Controllers/` defines HTTP endpoints; `Services/` owns business logic; `Repository/` handles data access (EF Core + Dapper). Review `TicketPlatFormServer/Repository/README.md` for connection and transaction rules.
- `DBModel/` contains EF entities; `DTO/` contains request/response contracts; `Config/` holds options bindings; `Common/` includes shared middleware and exceptions.
- `database_history/` stores SQL dumps/restore scripts; `Hubs/` holds SignalR hubs.
- `api_spec/` stores API specs; `scripts/setup.sh` bootstraps local setup.

## Build, Test, and Development Commands
- `dotnet restore` restores NuGet packages.
- `dotnet build` builds the solution.
- `dotnet run --project TicketPlatFormServer` runs the API (Swagger at `http://localhost:5224/swagger`).
- `scripts/setup.sh` installs local tooling and builds the project (macOS).
- `TicketPlatFormServer/database_history/db_restore.sh` or `TicketPlatFormServer/database_history/db_restore.bat` restores the MySQL dump.

## Coding Style & Naming Conventions
- Use 4-space indentation with braces on new lines, matching existing C# files.
- Naming: PascalCase for types/DTOs, `I` prefix for interfaces, `_camelCase` for private fields, `camelCase` for locals/parameters.
- Throw `AppException` for business-rule failures in services; avoid parallel repository calls due to scoped `IDbConnection`.

## Testing Guidelines
- No dedicated test project is in the repo yet. If you add tests, place them in `TicketPlatFormServer.Tests` and run `dotnet test`.
- Keep tests deterministic and focused on service/repository behavior.

## Commit & Pull Request Guidelines
- Commit messages follow conventional commits such as `feat: ...`, `chore: ...`, `docs: ...` with short, imperative summaries.
- PRs should include a clear description, testing notes, DB/migration impact, and updated API specs in `TicketPlatFormServer/api_spec` when endpoints change.

## Configuration & Secrets
- Local settings live in `TicketPlatFormServer/appsettings.json`, `TicketPlatFormServer/appsettings.Development.json`, and `TicketPlatFormServer/appsettings.SupabaseStorage.json`.
- Do not commit real credentials; document required new keys in the PR description when config changes.
