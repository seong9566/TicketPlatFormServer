---
trigger: always_on
---

# Testing Strategy

## Unit Tests
- **Scope**: Services, Logic-heavy components.
- **Mocking**: Mock Repository dependencies (use `Moq`).
- **Framework**: `xUnit`.

## Integration Tests
- **Scope**: Repositories.
- **Database**: Use an actual database (or Testcontainers) to verify SQL/EF logic.
- **Avoid**: Do not mock DbContext for repository tests if possible; test against real DB behavior.
