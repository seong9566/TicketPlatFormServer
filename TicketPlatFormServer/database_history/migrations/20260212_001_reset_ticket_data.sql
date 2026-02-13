-- ================================================================
-- Ticket Data Reset Script
-- Created: 2026-02-12
-- Purpose: Clean up all ticket-related data and insert fresh seed data
--          with correct unit prices after price calculation fix
-- ================================================================

SET FOREIGN_KEY_CHECKS = 0;

-- ================================================================
-- STEP 1: Delete all ticket-related data (cascading)
-- ================================================================

-- Delete transaction items first (references tickets)
DELETE FROM transaction_items WHERE ticket_id IS NOT NULL;

-- Delete transactions that are now orphaned
DELETE FROM transactions 
WHERE id NOT IN (SELECT DISTINCT transaction_id FROM transaction_items WHERE transaction_id IS NOT NULL);

-- Delete chat rooms referencing tickets
DELETE FROM chat_rooms WHERE ticket_id IS NOT NULL;

-- Delete user favorites for tickets
DELETE FROM user_favorites WHERE favorite_type_id = 2;

-- Delete ticket images
DELETE FROM ticket_images;

-- Finally, delete all tickets
DELETE FROM tickets;

-- Reset auto_increment counter
ALTER TABLE tickets AUTO_INCREMENT = 1;
ALTER TABLE ticket_images AUTO_INCREMENT = 1;

-- ================================================================
-- STEP 2: Insert fresh ticket seed data with correct unit prices
-- ================================================================

-- Note: All prices are now UNIT prices (per ticket), not total prices
-- Formula: Unit Price = Total Price / Quantity

-- Event 1: 아이유 콘서트 (event_id=1)
INSERT INTO tickets (seller_id, event_id, schedule_id, category_id, event_datetime, seat_location_id, area_id, `row`, quantity, remaining_quantity, price, description, status_id, seat_grade_id, trade_method_id, has_ticket, feature_ids) VALUES
(7, 1, 'SCH001A', 1, '2026-01-28 19:00:00', 1, 1, '5열', 2, 2, 110000, 'VIP석 연석 2장 판매합니다', 1, 1, 2, 1, '2,4'),
(7, 1, 'SCH001A', 1, '2026-01-28 19:00:00', 1, 2, '10열', 4, 4, 75000, 'R석 4장 일괄 판매', 1, 2, 1, 1, '2'),
(8, 1, 'SCH001A', 1, '2026-01-28 19:00:00', 1, 3, '15열', 3, 3, 50000, 'S석 3장 판매합니다', 1, 3, 2, 1, '4'),
(7, 1, 'SCH001B', 1, '2026-01-29 19:00:00', 1, 1, '3열', 2, 0, 115000, 'VIP석 연석 (매진)', 3, 1, 2, 1, '2,4'),
(8, 1, 'SCH001B', 1, '2026-01-29 19:00:00', 1, 2, '12열', 5, 5, 75000, 'R석 5장 판매', 1, 2, 1, 1, '2');

-- Event 2: Bunnies Camp 2024 (event_id=2)
INSERT INTO tickets (seller_id, event_id, schedule_id, category_id, event_datetime, seat_location_id, area_id, `row`, quantity, remaining_quantity, price, description, status_id, seat_grade_id, trade_method_id, has_ticket, feature_ids) VALUES
(7, 2, 'SCH002A', 1, '2026-02-23 18:00:00', 1, 4, '8열', 10, 8, 90000, 'VIP 입장권 10장', 1, 4, 2, 1, '2'),
(8, 2, 'SCH002A', 1, '2026-02-23 18:00:00', 1, 5, '20열', 15, 15, 50000, '일반 입장권 15장', 1, 5, 1, 1, '4'),
(9, 2, 'SCH002B', 1, '2026-02-24 18:00:00', 1, 4, '10열', 8, 5, 90000, 'VIP 입장권 8장', 1, 4, 2, 1, '2'),
(7, 2, 'SCH002B', 1, '2026-02-24 18:00:00', 1, 5, '25열', 20, 18, 50000, '일반 입장권 20장', 1, 5, 1, 1, '4');

-- Event 3: 시티팝 페스티벌 (event_id=3)
INSERT INTO tickets (seller_id, event_id, schedule_id, category_id, event_datetime, seat_location_id, area_id, `row`, quantity, remaining_quantity, price, description, status_id, seat_grade_id, trade_method_id, has_ticket, feature_ids) VALUES
(7, 3, 'SCH003A', 1, '2026-08-02 17:00:00', 2, 6, '스탠딩A', 30, 25, 66000, '스탠딩 A구역 30매', 1, 6, 1, 1, '2'),
(8, 3, 'SCH003A', 1, '2026-08-02 17:00:00', 1, 7, '5열', 10, 8, 82500, '지정석 10매 판매', 1, 7, 2, 1, '2,4'),
(9, 3, 'SCH003B', 1, '2026-08-03 17:00:00', 2, 6, '스탠딩B', 50, 50, 66000, '스탠딩 B구역 50매', 1, 6, 1, 1, '4'),
(7, 3, 'SCH003B', 1, '2026-08-03 17:00:00', 1, 7, '8열', 15, 10, 82500, '지정석 15매', 1, 7, 2, 1, '2');

-- Event 4: 뮤지컬 <시카고> (event_id=4)
INSERT INTO tickets (seller_id, event_id, schedule_id, category_id, event_datetime, seat_location_id, area_id, `row`, quantity, remaining_quantity, price, description, status_id, seat_grade_id, trade_method_id, has_ticket, feature_ids) VALUES
(8, 4, 'SCH004A', 3, '2026-03-14 18:00:00', 1, 1, '7열', 4, 2, 125000, 'VIP석 4장 판매', 1, 1, 2, 1, '2,4'),
(9, 4, 'SCH004A', 3, '2026-03-14 18:00:00', 1, 2, '15열', 6, 6, 90000, 'R석 6장 일괄', 1, 2, 1, 1, '2'),
(7, 4, 'SCH004B', 3, '2026-03-15 14:00:00', 1, 3, '20열', 8, 8, 70000, 'S석 8장 판매', 1, 3, 1, 1, '4');

-- Event 6: 뉴진스 월드투어 (event_id=6)
INSERT INTO tickets (seller_id, event_id, schedule_id, category_id, event_datetime, seat_location_id, area_id, `row`, quantity, remaining_quantity, price, description, status_id, seat_grade_id, trade_method_id, has_ticket, feature_ids) VALUES
(7, 6, 'SCH006A', 1, '2026-10-28 19:00:00', 2, 6, '스탠딩', 50, 30, 90000, '스탠딩 50매 대량 판매', 1, 6, 1, 1, '2'),
(8, 6, 'SCH006A', 1, '2026-10-28 19:00:00', 1, 1, '3열', 3, 3, 150000, 'VIP석 3연석 판매', 1, 1, 2, 1, '2,4'),
(9, 6, 'SCH006B', 1, '2026-10-29 19:00:00', 2, 6, '스탠딩', 40, 35, 90000, '스탠딩 40매', 1, 6, 1, 1, '4');

-- Event 7: 세븐틴 팬미팅 (event_id=7)
INSERT INTO tickets (seller_id, event_id, schedule_id, category_id, event_datetime, seat_location_id, area_id, `row`, quantity, remaining_quantity, price, description, status_id, seat_grade_id, trade_method_id, has_ticket, feature_ids) VALUES
(7, 7, 'SCH007A', 1, '2026-03-14 14:00:00', 1, 1, '10열', 2, 2, 90000, 'VIP석 2연석 판매', 1, 1, 2, 1, '2,4'),
(8, 7, 'SCH007A', 1, '2026-03-14 14:00:00', 1, 2, '18열', 5, 5, 65000, 'R석 5장 판매', 1, 2, 1, 1, '2'),
(9, 7, 'SCH007B', 1, '2026-03-15 14:00:00', 1, 3, '25열', 10, 10, 45000, 'S석 10장 판매', 1, 3, 1, 1, '4');

SET FOREIGN_KEY_CHECKS = 1;

-- ================================================================
-- Verification queries
-- ================================================================

SELECT 'Ticket Count:' AS Info, COUNT(*) AS Total FROM tickets;
SELECT 'Active Tickets:' AS Info, COUNT(*) AS Total FROM tickets WHERE status_id = 1;
SELECT 'Ticket Images:' AS Info, COUNT(*) AS Total FROM ticket_images;
SELECT 'Chat Rooms with Tickets:' AS Info, COUNT(*) AS Total FROM chat_rooms WHERE ticket_id IS NOT NULL;
SELECT 'Favorites:' AS Info, COUNT(*) AS Total FROM user_favorites WHERE favorite_type_id = 2;

-- Sample price verification (should show unit prices)
SELECT 
    id,
    event_id,
    quantity,
    price AS unit_price,
    (price * quantity) AS total_price,
    description
FROM tickets
LIMIT 5;
