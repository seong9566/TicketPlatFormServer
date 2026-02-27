# PROJECT KNOWLEDGE BASE (Repository Layer)

## OVERVIEW
Data-access layer for TicketHub backend. Hybrid EF Core + Dapper. 28 interface/implementation pairs + ReadModels projections. Must remain data-access only — no business logic, no transaction management.

## STRUCTURE
```text
Repository/
├── TicketContext.cs         # EF Core DbContext; 70+ DbSet properties; lives here (repository concern)
├── ReadModels/              # 13 Dapper projection classes (result shapes for complex queries)
├── Balance/                 IBalanceRepository, BalanceRepository
├── BankAccount/             IBankAccountRepository, BankAccountRepository
├── Chat/                    IChatRepository, ChatRepository, ChatQueries.cs
├── Dispute/                 IDisputeRepository, DisputeRepository
├── Events/                  IEventRepository, EventRepository, EventQueries.cs
├── Favorite/                IFavoriteRepository, FavoriteRepository, FavoriteQueries.cs
├── Home/                    IHomeRepository, HomeRepository, HomeQueries.cs
├── Notification/            INotificationRepository, NotificationRepository,
│                            INotificationTokenRepository, NotificationTokenRepository
├── Payment/                 IPaymentRepository, PaymentRepository
├── Reputation/              IReputationRepository, ReputationRepository
├── Search/                  ISearchRepository, SearchRepository
├── Sell/                    ISellRepository, SellRepository, SellQueries.cs
├── Settlement/              ISettlementRepository, SettlementRepository
├── Ticket/                  ITicketRepository, TicketRepository, TicketQueries.cs
├── Token/                   IRefreshTokenRepository, RefreshTokenRepository
├── Transaction/             ITransactionRepository, TransactionRepository
├── User/                    IUserRepository, UserRepository
└── Withdrawal/              IWithdrawalRepository, WithdrawalRepository
```

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Connection safety rules | `README.md` | IDbConnection concurrency, `Task.WhenAll` forbidden |
| Complex query SQL | `*Queries.cs` files | SQL constants isolated beside their repository |
| Dapper projection targets | `ReadModels/` | Typed result classes for multi-join queries |
| EF schema + relations | `TicketContext.cs` | DbSet registrations + EF fluent config |
| Re-scaffold command | COMMANDS below | Use when MySQL schema changes |

## CONVENTIONS
- Accept and return entities or primitives only; no DTO contracts in repository interfaces.
- **EF Core** for: simple CRUD, single-table queries, relationship loading (`Include`), change tracking.
- **Dapper** for: multi-join (3+ tables), aggregates (`SUM`, `COUNT`, `GROUP BY`), high-volume reads.
- SQL constants go in a sibling `*Queries.cs` file (e.g., `ChatQueries.cs`) when queries grow large.
- Repository classes use C# 12 primary constructors: `public class ChatRepository(TicketContext db, IDbConnection dapper)`.
- Never open, close, or dispose the DI-provided scoped `IDbConnection`.

## ANTI-PATTERNS
- **CRITICAL**: Never execute repository calls in parallel in the same request scope (`Task.WhenAll` forbidden — single scoped `IDbConnection` cannot be shared).
- Never manage transactions in repository code; service layer calls `BeginTransactionAsync()`.
- Never pass DTOs into repository method signatures; use entities or primitives.
- Never add business validation or state rules in this layer.
- Never manually dispose DI-provided `IDbConnection`.

## TRANSACTION RULE
- Service starts EF transaction: `await using var tx = await _db.Database.BeginTransactionAsync();`
- Dapper shares the same transaction: `transaction: tx.GetDbTransaction()`
- Service commits/rolls back; repository is transaction-unaware.

## COMMANDS
```bash
# from TicketPlatFormServer/
dotnet build --project TicketPlatFormServer.sln

# Re-scaffold when MySQL schema changes:
dotnet ef dbcontext scaffold "<connection-string>" Pomelo.EntityFrameworkCore.MySql \
  --output-dir DBModel --context-dir Repository --context TicketContext --force
```

## NOTES
- `TicketContext.cs` lives in `Repository/`, not `DBModel/` — intentional; context is a repository concern.
- Primary risk in this layer is connection concurrency misuse; review `README.md` for full examples.
- After re-scaffolding, review navigation property changes — EF may alter existing `Include` chains.
- If adding a new domain repository, follow existing folder naming convention and register in `Program.cs`.
