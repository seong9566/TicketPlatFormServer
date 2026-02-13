SET @confirmed_status_id := (
    SELECT id
    FROM transaction_statuses
    WHERE code = 'confirmed' AND is_active = 1
    ORDER BY id
    LIMIT 1
);

SET @pending_payment_status_id := (
    SELECT id
    FROM transaction_statuses
    WHERE code = 'pending_payment' AND is_active = 1
    ORDER BY id
    LIMIT 1
);

SET @paid_status_id := (
    SELECT id
    FROM transaction_statuses
    WHERE code = 'paid' AND is_active = 1
    ORDER BY id
    LIMIT 1
);

UPDATE transactions t
INNER JOIN escrow e ON e.transaction_id = t.id
SET t.status_id = @confirmed_status_id,
    t.confirmed_at = COALESCE(t.confirmed_at, e.released_at, NOW())
WHERE @confirmed_status_id IS NOT NULL
  AND t.deleted_at IS NULL
  AND e.released_at IS NOT NULL
  AND (t.confirmed_at IS NULL OR t.status_id <> @confirmed_status_id)
  AND (t.status_id = @pending_payment_status_id OR t.status_id = @paid_status_id);

UPDATE transactions t
INNER JOIN chat_rooms cr ON cr.transaction_id = t.id
SET t.status_id = @confirmed_status_id,
    t.confirmed_at = COALESCE(t.confirmed_at, cr.locked_at, NOW())
WHERE @confirmed_status_id IS NOT NULL
  AND t.deleted_at IS NULL
  AND cr.locked_at IS NOT NULL
  AND (t.confirmed_at IS NULL OR t.status_id <> @confirmed_status_id)
  AND (t.status_id = @pending_payment_status_id OR t.status_id = @paid_status_id);

SELECT t.id, ts.code AS status_code, t.confirmed_at
FROM transactions t
LEFT JOIN transaction_statuses ts ON ts.id = t.status_id
WHERE t.id IN (
    SELECT DISTINCT transaction_id
    FROM chat_rooms
    WHERE transaction_id IS NOT NULL
)
ORDER BY t.id DESC
LIMIT 50;
