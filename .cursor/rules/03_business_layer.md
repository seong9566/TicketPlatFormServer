---
trigger: always_on
---

# Business Layer (Services & DTOs)

## Services
- **Role**: Handle business logic, rules, and DTO <-> Entity conversion.
- **Operations**: Orchestrate calls to Repositories.
- **Validation**: Validate enums, specific business rules.
- **Security**: encrypt passwords (BCrypt) here.
- **Errors**: Throw `AppException` for business violations.
- **Restrictions**: NEVER perform DB operations directly. Always use a Repository.

## DTO Handling
- **Separation**: Create distinct DTOs for Request and Response (e.g., `RegisterUserReqDto`, `RegisterUserRespDto`).
- **Encapsulation**: Never expose internal entity structures (EF entities) to the client.
- **Validation**: DTO validation should be handled at the Controller level (attributes) or Service entry.
