# PROJECT KNOWLEDGE BASE (Controllers Layer)

## OVERVIEW
Thin HTTP endpoint layer for TicketHub API. 19 controllers — each delegates all logic to services, no business code.

## STRUCTURE
```text
Controllers/
├── AuthController.cs              # login, signup, logout, social OAuth (Kakao/Google), token refresh, find-id/pw
├── UserController.cs              # profile read/update, password change, verification
├── EventController.cs             # event listing, detail, category filter
├── TicketController.cs            # ticket browse, detail
├── SellController.cs              # 6-step ticket registration, my-tickets management
├── ChatController.cs              # chat rooms, messages, transaction state machine (request/confirm/cancel)
├── PaymentController.cs           # Toss Payments request/confirm/cancel
├── NotificationController.cs      # FCM token register/delete, notification list/read/read-all/unread-count
├── DisputeController.cs           # dispute create/detail/evidence upload/cancel
├── HomeController.cs              # home feed aggregation
├── FavoriteController.cs          # wishlist add/remove/list
├── BankAccountController.cs       # bank registration + 1-won verification
├── ReputationController.cs        # reputation ratings post-transaction
├── TransactionController.cs       # purchase/sale history
├── SettlementController.cs        # settlement status queries
├── WithdrawalController.cs        # seller withdrawal requests
├── BalanceController.cs           # balance queries
├── AdminBalanceController.cs      # admin escrow/balance actions
└── Search/SearchController.cs     # ticket keyword search
```

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Auth/token routes | `AuthController.cs` | JWT issue + refresh + social (Kakao/Google) |
| Payment flow | `PaymentController.cs` | delegates to PaymentService; Toss integration |
| Chat transaction state | `ChatController.cs` | confirm-purchase, cancel, request-payment actions |
| Dispute evidence upload | `DisputeController.cs` | file upload routed through DisputeService |
| FCM token management | `NotificationController.cs` | register/delete token + unread count |

## CONVENTIONS
- Controllers are thin: validate input shape → call one service method → return `ApiResponse<T>`.
- `[Authorize]` at class level; opt-out per action with `[AllowAnonymous]`.
- Extract user identity via `ClaimsExtensions`: `User.GetUserId()`, `User.GetEmail()`, `User.GetRole()`. Never parse claims manually.
- Return type: `ActionResult<ApiResponse<T>>` — never naked entities or raw DTOs.
- Route prefix: `[Route("api/[controller]")]` — must match constants in mobile `ApiEndpoint`.
- Preserve `*Async` suffix on all action methods.
- XML doc comments are Korean; match style when adding new actions.

## ANTI-PATTERNS
- Never put business logic, DB access, or validation in controllers.
- Never call repositories directly (always go through services).
- Never build error responses manually — throw `AppException`; `GlobalExceptionMiddleware` handles serialization.
- Never map entities or DB models to DTOs in controllers — service layer owns all mapping.
- Never leak `DbContext` or scoped connections into controller scope.

## NOTES
- `ClaimsExtensions.cs` in `Common/` is shared by controllers and `ChatHub`.
- `GlobalExceptionMiddleware` catches all `AppException` and converts to `ApiResponse<object>` with correct HTTP status.
- When adding a new controller, add corresponding path constant to mobile `lib/core/network/api_endpoint.dart`.
- `WithdrawalController`, `BalanceController`, `AdminBalanceController` are newer; follow same thin-delegate pattern.
