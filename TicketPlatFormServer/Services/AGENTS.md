# PROJECT KNOWLEDGE BASE (Service Layer)

## OVERVIEW
Business logic boundary for TicketHub backend. 25 domain service directories + BackgroundServices + Common. Orchestrates repositories, enforces rules, converts exceptions to `AppException`.

## STRUCTURE
```text
Services/
├── Artist/          IArtistService
├── Auth/            ITokenService, IUserService, IGoogleOAuthService, IKakaoOAuthService
├── Balance/         IBalanceService
├── BankAccount/     IBankAccountService
├── Chat/            IChatService (largest, high-risk — realtime + transaction coupling)
├── Common/          EncryptionService
├── Dispute/         IDisputeService (file upload + status management)
├── Email/           SmtpEmailService
├── Event/           IEventService
├── Favorite/        IFavoriteService
├── FileUpload/      IFileUploadService, ISupabaseStorageUploader, ISignedUrlCacheService
├── Home/            IHomeService
├── Notification/    INotificationService, IFcmService
├── Payment/         IPaymentService, ITossPaymentsService (high-risk — idempotency + external Toss API)
├── Reputation/      IReputationService
├── Search/          ISearchService
├── Sell/            ISellService (ticket registration + validation)
├── Settlement/      ISettlementService
├── Storage/         ISupabaseStorageUploader
├── Ticket/          ITicketService
├── Token/           ITokenService
├── Transaction/     ITransactionService
├── User/            IUserService (profile, password, image upload, verification)
├── Withdrawal/      IWithdrawalService
└── BackgroundServices/
    ├── ChatCleanupService.cs                      # Expired chat room cleanup
    ├── TransactionReservationCleanupService.cs     # Release expired reservations
    ├── TransactionAutoConfirmService.cs            # Auto-confirm after timeout
    ├── SettlementProcessingService.cs              # D+3 settlement batch
    └── WithdrawalProcessingService.cs              # Withdrawal batch processing
```

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Exception pattern details | `README.md` | InnerException usage, `AppException` decision tree |
| Chat workflow | `Chat/ChatService.cs` | Largest service (1157 lines); realtime + transaction state machine |
| Payment workflow | `Payment/PaymentService.cs` | 929 lines; idempotency, Toss API, escrow transitions |
| Toss API client | `Payment/TossPaymentsService.cs` | HTTP client, confirmation, cancellation, webhook |
| User operations | `User/UserService.cs` | 707 lines; profile, password, image upload, verification |
| Sell operations | `Sell/SellService.cs` | 691 lines; ticket registration wizard, category queries |
| Dispute file upload | `Dispute/DisputeService.cs` | Supabase Storage integration |
| FCM sending | `Notification/FcmService.cs` | HTTP v1 API; JWT token caching; UNREGISTERED cleanup |
| Notification triggers | `Notification/NotificationService.cs` | 4 triggers: CHAT_MESSAGE, PAYMENT_REQUEST, PAYMENT_SUCCESS, PURCHASE_CONFIRMED |
| Settlement processing | `BackgroundServices/SettlementProcessingService.cs` | Hosted service, runs on schedule |

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
- `ChatService.cs` (1157 lines, 22 methods) and `PaymentService.cs` (929 lines) are high-change/high-risk; edit surgically.
- `UserService.cs` (707 lines) has file compensation pattern: delete uploaded file in catch block on DB failure.
- `FcmService.cs` caches Google access tokens; don't break token refresh lifecycle.
- `SmtpEmailService.cs` uses `#pragma warning disable SYSLIB0027` (obsolete SmtpClient — intentional).
- `FileUploadService.cs` and `SupabaseStorageUploader.cs` use `#pragma warning disable CS0618` (obsolete BucketName — intentional).
- Reuse existing status-code lookup patterns (TransactionStatus, EscrowStatus) before introducing new ones.
- Background services use `IServiceProvider.CreateScope()` to avoid connection conflicts; implement retry with exponential backoff.

## COMMANDS
```bash
# from TicketPlatFormServer/
dotnet build --project TicketPlatFormServer.sln
dotnet run --project TicketPlatFormServer/TicketPlatFormServer.csproj
dotnet test --project TicketPlatFormServer.Tests/TicketPlatFormServer.Tests.csproj
```

## NOTES
- Dispute notification triggers (`DISPUTE_OPENED`, `DISPUTE_RESOLVED`) are pending Dispute service completion.
- File compensation pattern: if DB save fails after file upload, delete uploaded file in catch block (see `UserService.cs`).
- `SellService.cs` has TODO: icon URL mapping pending (`// TODO: 아이콘 URL 추가 시 매핑`).
- `UserService.cs` has TODO: orphan file cleanup needed (`// TODO: 주기적 정리 작업으로 고아 파일 제거 필요`).
