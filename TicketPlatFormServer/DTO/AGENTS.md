# PROJECT KNOWLEDGE BASE (DTO Layer)

## OVERVIEW
Data Transfer Objects for TicketHub API surface. 21 domain subdirectories, ~200 classes. Strict boundary: DTOs live only at controller/service boundary — never in repositories, never returned as entity types.

## STRUCTURE
```text
DTO/
├── ApiResponse.cs        # Generic wrapper: { success, statusCode, message, data: T }
├── Auth/                 # RegisterUserReqDto, LoginUserReqDto, LoginUserRespDto, SocialLoginReqDto...
├── Admin/                # Admin action DTOs
├── Artist/               # Artist listing/follow DTOs
├── Balance/              # Balance query DTOs
├── BankAccount/          # RegisterBankAccountReqDto, VerifyBankAccountReqDto...
├── Chat/                 # ChatRoomRespDto, SendMessageReqDto, MessageRespDto... (17 files)
├── Dispute/              # CreateDisputeReqDto, DisputeDetailRespDto, EvidenceUploadReqDto...
├── Event/                # EventListRespDto, EventDetailRespDto, CategoryFilterReqDto...
├── Favorite/             # FavoriteReqDto, FavoriteListRespDto
├── Home/                 # HomeRespDto (aggregated feed)
├── Notification/         # NotificationListRespDto, RegisterFcmTokenReqDto... (7 files)
├── Payment/              # PaymentRequestReqDto, PaymentConfirmReqDto, PaymentRespDto... (8 files)
├── Reputation/           # CreateReputationReqDto, ReputationRespDto
├── Search/               # SearchReqDto, SearchResultRespDto
├── Sell/                 # CreateSellTicketReqDto, SellTicketRespDto... (18 files — most complex)
├── Settlement/           # SettlementListRespDto, SettlementDetailRespDto
├── Ticket/               # TicketListRespDto, TicketDetailRespDto
├── Transaction/          # TransactionHistoryRespDto, TransactionDetailRespDto
├── User/                 # UserProfileRespDto, UpdateProfileReqDto... (9 files)
└── Withdrawal/           # WithdrawalReqDto, WithdrawalListRespDto
```

## CONVENTIONS
- **Naming**: Request DTOs = `*ReqDto`; Response DTOs = `*RespDto`. Never deviate.
- **Wrapper**: All API responses use `ApiResponse<T>` — controller constructs it, never service.
- **Scope**: DTOs flow: Controller (receives `ReqDto`) → Service (maps to entity/model) → Controller (wraps `RespDto` in `ApiResponse<T>`).
- **Immutability**: Keep DTOs as simple data containers; no business logic, no validation attributes beyond `[Required]`/`[Range]`.
- XML doc comments are Korean when present; preserve language style.

## ANTI-PATTERNS
- Never pass `ReqDto` into repository method signatures — convert to entity or primitive in service.
- Never return `RespDto` from repository methods — repositories return entities/primitives only.
- Never add business logic or computed fields to DTOs.
- Never share DTOs across multiple controllers/domains without explicit intent (prefer domain-specific DTOs).
- Never expose `DBModel` entity types as DTO fields.

## NOTES
- `Sell/` is the most complex domain (18 files) due to the 6-step wizard with multiple intermediate states.
- `Chat/` has 17 files covering room listing, message pagination, and realtime event payloads.
- When adding a new DTO, mirror the naming in mobile's `data/dto/` layer for cross-team consistency.
- `ApiResponse.cs` at root of `DTO/` is the canonical response wrapper — do not create alternatives.
