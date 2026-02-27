# PROJECT KNOWLEDGE BASE (DBModel Layer)

## OVERVIEW
EF Core 9 entity classes scaffolded from MySQL schema. 70 files — largest code directory. Treat as read-only schema source; business logic never lives here.

## STRUCTURE
```text
DBModel/                   # EF-scaffolded POCOs, one file per DB table
├── User.cs, UserProfile.cs, UserVerification.cs, AuthProvider.cs, AuthRole.cs
├── Ticket.cs, TicketVerificationMethod.cs, TicketImage.cs
├── Event.cs, EventSchedule.cs, EventSeatGrade.cs, EventCategory.cs
├── Transaction.cs, TransactionStatus.cs, TransactionHistory.cs, TransactionItem.cs
├── Payment.cs, PaymentMethod.cs, PaymentStatus.cs, PaymentEasyPayDetail.cs
├── ChatRoom.cs, ChatMessage.cs, ChatMessageImage.cs, ChatRoomStatus.cs
├── Dispute.cs, DisputeEvidence.cs, DisputeStatus.cs, DisputeType.cs
├── Settlement.cs, Escrow.cs, EscrowStatus.cs
├── Balance.cs, BalanceTransaction.cs
├── BankAccount.cs, Withdrawal.cs, WithdrawalStatus.cs
├── Notification.cs, NotificationToken.cs
├── Reputation.cs, Favorite.cs, FavoriteType.cs
├── AdminAction.cs, AdminActionType.cs
├── Artist.cs, ArtistFollower.cs
└── RefreshToken.cs
```

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Schema source of truth | `TicketContext.cs` (in `Repository/`) | DbContext with all DbSet registrations |
| Entity relationships | Individual entity files | Navigation properties reflect FK structure |
| Re-scaffolding procedure | `Repository/AGENTS.md` → COMMANDS | Use `dotnet ef dbcontext scaffold` when schema changes |

## CONVENTIONS
- Entities are plain POCOs: only auto-properties + navigation properties. No methods.
- Naming follows DB column names: snake_case → PascalCase via EF scaffolding.
- Nullable reference types enabled: `string?` = nullable column, `string` = NOT NULL column.
- `ICollection<T>` on parent entity = one-to-many; use `.Include()` in EF queries.
- Status lookup tables (TransactionStatus, ChatRoomStatus, EscrowStatus, etc.) have `code` + `name_ko` columns.

## ANTI-PATTERNS
- Never add methods, computed properties, or business logic to entity classes.
- Never reference DTOs or service types from this layer.
- Never manually edit scaffolded files unless adding EF fluent config — re-scaffold when schema drifts.
- Never use entities above the repository boundary (entities must not reach controllers or services as response types).

## NOTES
- `TicketContext.cs` lives in `Repository/`, not here — intentional (context is a repository concern).
- Re-scaffold command: see `Repository/AGENTS.md` COMMANDS section.
- After re-scaffolding, review navigation property changes carefully; EF may alter existing `Include` chains.
- Status entities (e.g., `TransactionStatus`) store `code` string identifiers — match against `Enum/` values when filtering.
