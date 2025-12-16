---
trigger: always_on
---

# Infrastructure & Cross-Cutting Concerns

## Error Handling
- **Custom Exceptions**: Use `AppException` for domain-specific errors.
- **Status Codes**: Map exceptions to appropriate HTTP status codes.
- **Global Handling**: Catch unhandled exceptions in middleware/filters and return standardized responses.

## Logging
- **Structured**: Use `ILogger<T>`.
- **Context**: Log exceptions with sufficient context.
- **Sensitivity**: NEVER log passwords, tokens, or PII.

## Security
- **Hashing**: Use BCrypt (via BCrypt.Net-Next) for passwords.
- **Validation**: rigorously validate inputs (enums, strings).
- **Storage**: Do not store plain text passwords.

## Swagger
- **Documentation**: Document all public endpoints with XML comments.
- **Attributes**: Annotate DTOs for better schema representation.

## Deployment
- Structure app for Docker containerization.
