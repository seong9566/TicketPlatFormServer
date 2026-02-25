# PROJECT KNOWLEDGE BASE (Controllers Layer)

## OVERVIEW
HTTP endpoint layer for TicketHub API. 15 domain controllers, each thin — delegate all logic to services.

## STRUCTURE
```text
Controllers/
├── AuthController.cs         # login, signup, logout, social, token refresh
├── UserController.cs         # profile read/update, password change
├── EventController.cs        # event listing and detail
├── TicketController.cs       # ticket browse and detail
├── SellController.cs         # ticket registration (multi-step), my-tickets
├── ChatController.cs         # chat rooms, messages, transaction flow
├── PaymentController.cs      # Toss Payments request/confirm/cancel
├── NotificationController.cs # FCM token, notification list, read
├── DisputeController.cs      # dispute create/detail/evidence/cancel
├── HomeController.cs         # home feed aggregation
├── FavoriteController.cs     # wishlist add/remove
├── BankAccountController.cs  # bank registration and verify
├── ReputationController.cs   # reputation ratings
├── TransactionController.cs  # purchase history
└── SettlementController.cs   # settlement queries
```

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Auth/token routes | `AuthController.cs` | JWT issue + refresh + social (Kakao/Google) |
| Payment flow | `PaymentController.cs` | delegates to PaymentService; Toss integration |
| Chat transaction state | `ChatController.cs` | confirm-purchase, cancel, request-payment actions |
| Dispute evidence upload | `DisputeController.cs` | file upload routed through service |

## CONVENTIONS
- Controllers are thin: validate input shape, call one service method, return `ApiResponse<T>`.
- Use `[Authorize]` at class level; opt-out per action with `[AllowAnonymous]`.
- Extract user identity via `ClaimsExtensions` (`User.GetUserId()`), never parse claims manually.
- Return types: `ActionResult<ApiResponse<T>>` — never naked entities or DTOs.
- Route prefix: `[Route("api/[controller]")]` — matches `ApiEndpoint` constants in mobile.

## ANTI-PATTERNS
- Never add business logic, validation, or DB access in controllers.
- Never call repositories directly from controllers.
- Never build error responses manually — throw `AppException`; `GlobalExceptionMiddleware` handles the rest.
- Never map entities to DTOs in controllers — service layer owns that mapping.

## NOTES
- `ClaimsExtensions.cs` lives in `Hubs/` but is shared across controllers.
- When adding a new controller, add corresponding `ApiEndpoint` constant in mobile `lib/core/network/api_endpoint.dart`.
