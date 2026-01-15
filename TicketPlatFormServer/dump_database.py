#!/usr/bin/env python3
"""
Database dump script for TicketPlatForm
Generates SQL dump file with schema and data
"""
import mysql.connector
from datetime import datetime
import os

# Database connection config
DB_CONFIG = {
    'host': 'localhost',
    'port': 3306,
    'database': 'TicketPlatFormDB',
    'user': 'root',
    'password': 'stecdev1234!'
}

def get_create_table(cursor, table_name):
    """Get CREATE TABLE statement for a table"""
    cursor.execute(f"SHOW CREATE TABLE `{table_name}`")
    result = cursor.fetchone()
    return result[1] if result else None

def get_table_data(cursor, table_name):
    """Get INSERT statements for table data"""
    cursor.execute(f"SELECT * FROM `{table_name}`")
    rows = cursor.fetchall()

    if not rows:
        return []

    # Get column names
    columns = [desc[0] for desc in cursor.description]
    insert_statements = []

    for row in rows:
        values = []
        for value in row:
            if value is None:
                values.append('NULL')
            elif isinstance(value, str):
                # Escape single quotes and backslashes
                escaped = value.replace('\\', '\\\\').replace("'", "\\'")
                values.append(f"'{escaped}'")
            elif isinstance(value, (int, float)):
                values.append(str(value))
            elif isinstance(value, datetime):
                values.append(f"'{value.strftime('%Y-%m-%d %H:%M:%S')}'")
            elif isinstance(value, bytes):
                # Handle binary data
                values.append(f"0x{value.hex()}")
            else:
                values.append(f"'{str(value)}'")

        column_list = ', '.join(f'`{col}`' for col in columns)
        values_list = ', '.join(values)
        insert_statements.append(f"INSERT INTO `{table_name}` ({column_list}) VALUES ({values_list});")

    return insert_statements

def main():
    # Create output directory
    output_dir = '../database_history'
    os.makedirs(output_dir, exist_ok=True)

    # Generate filename with timestamp
    timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
    output_file = os.path.join(output_dir, f'dump_{timestamp}.sql')

    try:
        # Connect to database
        conn = mysql.connector.connect(**DB_CONFIG)
        cursor = conn.cursor()

        # Get all tables
        cursor.execute("SHOW TABLES")
        tables = [row[0] for row in cursor.fetchall()]

        print(f"Found {len(tables)} tables to dump")

        with open(output_file, 'w', encoding='utf-8') as f:
            # Write header
            f.write(f"-- MySQL dump for TicketPlatFormDB\n")
            f.write(f"-- Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
            f.write(f"-- Database: {DB_CONFIG['database']}\n")
            f.write(f"-- Tables: {len(tables)}\n")
            f.write(f"--\n\n")

            f.write("SET NAMES utf8mb4;\n")
            f.write("SET FOREIGN_KEY_CHECKS = 0;\n\n")

            # Process each table
            for i, table in enumerate(tables, 1):
                print(f"Processing {i}/{len(tables)}: {table}")

                f.write(f"\n-- ----------------------------\n")
                f.write(f"-- Table structure for {table}\n")
                f.write(f"-- ----------------------------\n")
                f.write(f"DROP TABLE IF EXISTS `{table}`;\n")

                # Get CREATE TABLE statement
                create_sql = get_create_table(cursor, table)
                if create_sql:
                    f.write(f"{create_sql};\n\n")

                # Get table data
                f.write(f"-- ----------------------------\n")
                f.write(f"-- Records of {table}\n")
                f.write(f"-- ----------------------------\n")

                insert_statements = get_table_data(cursor, table)
                if insert_statements:
                    f.write('\n'.join(insert_statements))
                    f.write('\n\n')
                else:
                    f.write(f"-- No data in {table}\n\n")

            f.write("SET FOREIGN_KEY_CHECKS = 1;\n")

        cursor.close()
        conn.close()

        print(f"\n✓ Database dump created successfully:")
        print(f"  {output_file}")
        print(f"  Size: {os.path.getsize(output_file)} bytes")

    except Exception as e:
        print(f"✗ Error: {e}")
        return 1

    return 0

if __name__ == '__main__':
    exit(main())
