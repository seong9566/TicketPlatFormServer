# Draft: Payment Schema Mismatch Fix

## Problem Analysis

### Root Cause Identified
**Database Schema vs EF Entity Mismatch**

The `payments` table in the actual MySQL database DOES contain all the columns that EF Core is trying to SELECT:
- `merchant_id` ✓
- `api_version` ✓
- `country` ✓
- `culture_expense` ✓
- `discount_info` ✓
- `is_partial_cancelable` ✓
- `last_transaction_key` ✓
- `metadata` ✓
- `payment_type` ✓
- `use_escrow` ✓

**BUT**: The TicketContext.OnModelCreating() configuration (lines 1187-1249) does NOT map these columns.

### Evidence

**Database Schema** (from `database_history/TicketPlatFormDB_dump.sql`):
```sql
CREATE TABLE `payments` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `transaction_id` bigint NOT NULL,
  `pg_provider` varchar(50),
  `payment_key` varchar(255),
  `order_id` varchar(255),
  `amount` bigint unsigned NOT NULL,
  `method_id` bigint NOT NULL DEFAULT '1',
  `paid_at` datetime,
  `status_id` bigint NOT NULL DEFAULT '1',
  `use_escrow` tinyint(1) NOT NULL DEFAULT '0',
  `is_partial_cancelable` tinyint(1) NOT NULL DEFAULT '0',
  `payment_type` varchar(20),
  `last_transaction_key` varchar(255),
  `merchant_id` varchar(50),              -- ✓ EXISTS
  `api_version` varchar(20),              -- ✓ EXISTS
  `country` char(2) DEFAULT 'KR',         -- ✓ EXISTS
  `culture_expense` tinyint(1) DEFAULT '0', -- ✓ EXISTS
  `metadata` json,                        -- ✓ EXISTS
  `discount_info` json,                   -- ✓ EXISTS
  ...
)
```

**TicketContext Configuration** (lines 1187-1249):
Only maps these 9 columns:
- id
- amount
- method_id
- order_id
- paid_at
- payment_key
- pg_provider
- status_id
- transaction_id

**Missing 10 columns from OnModelCreating configuration!**

**Payment Entity** (DBModel/Payment.cs):
Contains ALL properties (lines 11-103), including the missing ones.

### Why This Happens

When EF Core tries to query Payment entity with `.Include()`:
1. Sees Payment entity has properties like `ApiVersion`, `Country`, etc.
2. Checks OnModelCreating configuration
3. Finds NO explicit mapping for these properties
4. Uses **convention-based mapping** (property name → snake_case column name)
5. Generates SQL: `SELECT p.ApiVersion AS ApiVersion, p.Country AS Country...`
6. MySQL ERROR: Column names don't match (should be `api_version`, `country`)

### Solution Strategy

**Option 1: Update TicketContext OnModelCreating** ✓ RECOMMENDED
- Add explicit column mappings for the 10 missing properties
- Aligns with existing pattern in the file
- No database changes needed
- Matches actual schema

**Option 2: Regenerate Entity from Database**
- Run EF Core scaffolding command
- RISK: Might overwrite manual customizations
- NOT RECOMMENDED (recent manual fix to Payment.Id type)

## Decisions

- **Approach**: Update TicketContext.OnModelCreating with missing column mappings
- **Scope**: Payment entity only (NOT touching other entities)
- **Verification**: Run `POST /api/payment/confirm` endpoint after fix
- **Rollback**: Git revert if anything breaks

## Missing Column Mappings Needed

1. `MerchantId` → `merchant_id`
2. `ApiVersion` → `api_version`
3. `Country` → `country`
4. `UseEscrow` → `use_escrow`
5. `IsPartialCancelable` → `is_partial_cancelable`
6. `PaymentType` → `payment_type`
7. `LastTransactionKey` → `last_transaction_key`
8. `CultureExpense` → `culture_expense`
9. `Metadata` → `metadata`
10. `DiscountInfo` → `discount_info`

## Files to Modify

- `TicketPlatFormServer/Repository/TicketContext.cs` (lines 1187-1249)

## Verification Commands

```bash
# 1. Build project
dotnet build

# 2. Run API
dotnet run --project TicketPlatFormServer

# 3. Test payment confirm endpoint
curl -X POST http://localhost:5224/api/payment/confirm \
  -H "Content-Type: application/json" \
  -d '{"orderId":"test","paymentKey":"test","amount":1000}'
```

## Test Infrastructure Assessment

- **Infrastructure exists**: NO (no test project found)
- **User wants tests**: TBD (ask user)
- **QA approach**: Manual verification via API endpoint

## Risk Assessment

- **LOW RISK**: Only adding missing EF Core mappings
- **No DB schema changes**: Database already correct
- **No data migration needed**: Existing data unaffected
- **Rollback**: Simple `git revert`
