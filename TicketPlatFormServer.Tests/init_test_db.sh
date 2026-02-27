#!/bin/bash
# Initialize test database manually
# Usage: ./init_test_db.sh

DB_HOST="127.0.0.1"
DB_PORT="3306"
DB_USER="root"
DB_PASS="stecdev1234!"
TEST_DB="TicketPlatFormDB_Test"
DB_HISTORY_DIR="../TicketPlatFormServer/database_history"

echo "Creating test database..."
mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASS -e "CREATE DATABASE IF NOT EXISTS \`$TEST_DB\` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"

echo "Applying dump (stripping GTID for test environment)..."
# Strip GTID_PURGED lines — dump was created from dev DB with GTIDs which conflict on restore
sed '/GTID_PURGED/d; /^[0-9a-f]\{8\}-/d' "$DB_HISTORY_DIR/TicketPlatFormDB_dump.sql" | \
  mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASS $TEST_DB

echo "Applying migrations..."
# TASK-008 uses ADD COLUMN IF NOT EXISTS (MariaDB syntax, MySQL 9.x invalid) — use --force
# Columns already exist from dump, so this is a safe no-op with --force
mysql --force -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASS $TEST_DB < "$DB_HISTORY_DIR/TASK-008-migration.sql"
mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASS $TEST_DB < "$DB_HISTORY_DIR/TASK-012-migration.sql"
mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASS $TEST_DB < "$DB_HISTORY_DIR/TASK-013-migration.sql"
mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASS $TEST_DB < "$DB_HISTORY_DIR/BALANCE-001-migration.sql"

echo "Done! Test DB initialized."
mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASS -e "SHOW DATABASES LIKE '$TEST_DB';"
