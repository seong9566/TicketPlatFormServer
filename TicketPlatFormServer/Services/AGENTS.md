# PROJECT KNOWLEDGE BASE (Service Layer)

## OVERVIEW
Business logic boundary for TicketHub backend. 28 service interface/implementation pairs + background services. Orchestrates repositories, enforces rules, converts exceptions to `AppException`.

## STRUCTURE
```text
Services/
├── Auth/           ITokenService, IUserService, IGoogleOAuthService, IKakaoOAuthService
├── Balance/        IBalanceService
├── BankAccount/    IBankAccountService
├── Chat/           IChatService (largest, high-risk — realtime + transaction coupling)
├── Dispute/        IDisputeService (file upload + status management)
├── Event/          IEventService
├── Favorite/       IFavoriteService
├── FileUpload/     IFileUploadService, ISupabaseStorageUploader, ISignedUrlCacheService
├── Home/           IHomeService
├── Notification/   INotificationService, IFcmService
├── Payment/        IPaymentService (high-risk — idempotency + external Toss API)
├── Reputation/     IReputationService
├── Search/         ISearchService
├── Sell/           ISellService (ticket registration + validation)
├── Settlement/     ISettlementService
├── Ticket/         ITicketService
├── Transaction/    ITransactionService
├── User/           IUserService
├── Withdrawal/     IWithdrawalService
├── Background/     ChatCleanupService, SettlementProcessingService (hosted services)
└── Common/         EncryptionService, SmtpEmailService
```

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Exception pattern details | `README.md` | InnerException usage, `AppException` decision tree |
| Chat workflow | `Chat/ChatService.cs` | Largest service; realtime + transaction state machine |
| Payment workflow | `Payment/PaymentService.cs` | Idempotency, Toss API integration, escrow transitions |
| Dispute file upload | `Dispute/DisputeService.cs` | Supabase Storage integration |
| FCM sending | `Notification/FcmService.cs` | HTTP v1 API; JWT token caching; UNREGISTERED cleanup |
| Notification triggers | `Notification/NotificationService.cs` | 4 triggers: CHAT_MESSAGE, PAYMENT_REQUEST, PAYMENT_SUCCESS, PURCHASE_CONFIRMED |
| Settlement processing | `Background/SettlementProcessingService.cs` | Hosted service, runs on schedule |

## CONVENTIONS
- Validate all business invariants in services, not controllers or repositories.
- Throw `AppException(message, statusCode)` for validation/business rule failures.
- Throw `AppException(message, statusCode, innerException)` when wrapping DB/external API exceptions (preserves root cause for logging).
- Re-throw `AppException` as-is when catching in nested try/catch — never swallow.
- Service layer owns transaction lifecycle: `BeginTransactionAsync()` → commit/rollback.
- Service layer owns all DTO ↔ entity mapping; repositories return raw entities.
- Preserve `*Async` suffix on all public methods.
- Korean language in XML doc comments; match style in touched files.

## ANTI-PATTERNS
- Never perform direct SQL or `DbContext` transaction orchestration in controllers.
- Never call repositories in parallel under scoped connection lifecycle (`Task.WhenAll` = connection conflict).
- Never leak DB entities to controller response directly — always map to DTO.
- Never swallow exceptions; preserve root cause via `innerException` when converting.
- Never add business logic in repositories (rule enforcement is service-only).

## QUALITY HOTSPOTS
- `ChatService.cs` and `PaymentService.cs` are high-change/high-risk; edit surgically.
- `FcmService.cs` caches Google access tokens; don't break token refresh lifecycle.
- `SettlementProcessingService.cs` is a hosted background service — test carefully.
- Reuse existing status-code lookup patterns (TransactionStatus, EscrowStatus) before introducing new ones.

## COMMANDS
```bash
# from TicketPlatFormServer/
dotnet build --project TicketPlatFormServer.sln
dotnet run --project TicketPlatFormServer/TicketPlatFormServer.csproj
```

## NOTES
- Backend currently has no dedicated test project; service-layer unit tests are the recommended first target.
- Dispute notification triggers (`DISPUTE_OPENED`, `DISPUTE_RESOLVED`) are pending Dispute service completion.
- File compensation pattern: if DB save fails after file upload, delete uploaded file in catch block (see `UserService.cs`).
