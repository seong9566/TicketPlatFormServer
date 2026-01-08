using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketPlatFormServer.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "admin_action_types",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true, comment: "한글 표시명", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'", comment: "활성화 여부"),
                    sort_order = table.Column<int>(type: "int", nullable: false, comment: "정렬 순서")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "관리자 액션 유형 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "admin_target_types",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true, comment: "한글 표시명", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "관리자 작업 대상 유형 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "auth_providers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "인증 제공자 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "auth_roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "사용자 역할 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "bank_account",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    bank_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, comment: "은행명", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    account_number = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, comment: "계좌번호", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    account_holder = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, comment: "예금주", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    verified = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'0'", comment: "계좌 인증 여부"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "사용자 은행 계좌 정보 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "chat_room_statuses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "채팅방 상태 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "dispute_statuses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "분쟁 상태 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "dispute_types",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "분쟁 유형 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "escrow_statuses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "에스크로 상태 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "favorite_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "찜 유형 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "notification_platforms",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "알림 플랫폼 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "notification_types",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "알림 유형 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "payment_methods",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "결제 수단 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "payment_statuses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "결제 상태 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "refund_reasons",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "환불 사유 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "refund_statuses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "환불 상태 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "reputation_rating_types",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "평판 평가 유형 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "settlement_statuses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "정산 상태 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "ticket_category",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "티켓 카테고리 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "ticket_images",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ticket_id = table.Column<long>(type: "bigint", nullable: false),
                    image_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "티켓 이미지 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "ticket_price_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ticket_id = table.Column<long>(type: "bigint", nullable: false),
                    old_price = table.Column<int>(type: "int", nullable: false, comment: "변경 전 가격"),
                    new_price = table.Column<int>(type: "int", nullable: false, comment: "변경 후 가격"),
                    reason = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, comment: "변경 사유", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    changed_by = table.Column<long>(type: "bigint", nullable: true, comment: "변경자 FK"),
                    changed_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "티켓 가격 변경 이력 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "ticket_statuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "티켓 상태 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "ticket_verification_methods",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "티켓 검증 방법 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "transaction_confirmed_bys",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "거래 확인자 유형 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "transaction_statuses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name_ko = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                },
                comment: "거래 상태 코드 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "user_profile",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    nickname = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, comment: "닉네임", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    profile_image_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, comment: "프로필 이미지 URL", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bio = table.Column<string>(type: "text", nullable: true, comment: "자기소개", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    manner_temperature = table.Column<float>(type: "float", nullable: true, defaultValueSql: "'36.5'", comment: "매너 온도 (36.5~99.9)"),
                    total_trade_count = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'", comment: "총 거래 횟수")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.user_id);
                },
                comment: "사용자 프로필 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "user_verification",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, comment: "실명", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    birth = table.Column<DateOnly>(type: "date", nullable: true, comment: "생년월일"),
                    identity_verified = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'0'", comment: "본인 인증 완료"),
                    phone_verified = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'0'", comment: "휴대폰 인증 완료"),
                    account_verified = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'0'", comment: "계좌 인증 완료"),
                    verified_at = table.Column<DateTime>(type: "timestamp", nullable: true, comment: "인증 완료 시각")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.user_id);
                },
                comment: "사용자 본인 인증 정보 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "admin_actions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    admin_id = table.Column<long>(type: "bigint", nullable: false, comment: "관리자 FK"),
                    action_type_id = table.Column<long>(type: "bigint", nullable: false, comment: "액션 유형 FK"),
                    target_type_id = table.Column<long>(type: "bigint", nullable: false, comment: "대상 유형 FK"),
                    target_id = table.Column<long>(type: "bigint", nullable: false, comment: "대상 ID"),
                    reason = table.Column<string>(type: "text", nullable: true, comment: "사유", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_admin_actions_action_type",
                        column: x => x.action_type_id,
                        principalTable: "admin_action_types",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_admin_actions_target_type",
                        column: x => x.target_type_id,
                        principalTable: "admin_target_types",
                        principalColumn: "id");
                },
                comment: "관리자 액션 로그 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    email = table.Column<string>(type: "varchar(255)", nullable: false, comment: "이메일 (로그인 ID)", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, comment: "비밀번호 해시 (소셜 로그인 시 NULL)", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, comment: "연락처", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    provider_id = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    role_id = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_login_at = table.Column<DateTime>(type: "timestamp", nullable: true, comment: "마지막 로그인 시각"),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'0'", comment: "탈퇴 여부 (Soft Delete)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_provider",
                        column: x => x.provider_id,
                        principalTable: "auth_providers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_users_role",
                        column: x => x.role_id,
                        principalTable: "auth_roles",
                        principalColumn: "id");
                },
                comment: "사용자 기본 정보 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "user_favorites",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    favorite_type_id = table.Column<int>(type: "int", nullable: false),
                    target_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_favorites_type",
                        column: x => x.favorite_type_id,
                        principalTable: "favorite_types",
                        principalColumn: "id");
                },
                comment: "사용자 찜 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "notification_token",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false, comment: "사용자 FK"),
                    device_token = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false, comment: "FCM/APNs 토큰", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    platform_id = table.Column<long>(type: "bigint", nullable: false, comment: "플랫폼 FK"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_notification_token_platform",
                        column: x => x.platform_id,
                        principalTable: "notification_platforms",
                        principalColumn: "id");
                },
                comment: "알림 디바이스 토큰 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false, comment: "수신자 FK"),
                    type_id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "'1'", comment: "알림 유형 FK"),
                    title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, comment: "알림 제목", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    body = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, comment: "알림 내용", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    read_flag = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'0'", comment: "읽음 여부"),
                    read_at = table.Column<DateTime>(type: "timestamp", nullable: true, comment: "읽은 시각"),
                    data = table.Column<string>(type: "json", nullable: true, comment: "추가 데이터 (페이로드)", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_type",
                        column: x => x.type_id,
                        principalTable: "notification_types",
                        principalColumn: "id");
                },
                comment: "알림 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "artists",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    profile_image_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "artists_ibfk_1",
                        column: x => x.category_id,
                        principalTable: "ticket_category",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    buyer_id = table.Column<long>(type: "bigint", nullable: false, comment: "구매자 FK"),
                    seller_id = table.Column<long>(type: "bigint", nullable: false, comment: "판매자 FK"),
                    status_id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "'1'", comment: "상태 FK"),
                    reserved_at = table.Column<DateTime>(type: "datetime", nullable: true, comment: "예약 시각"),
                    reservation_expires_at = table.Column<DateTime>(type: "datetime", nullable: true, comment: "예약 만료 시각"),
                    confirmed_at = table.Column<DateTime>(type: "datetime", nullable: true, comment: "구매 확정 시각"),
                    auto_confirm_at = table.Column<DateTime>(type: "datetime", nullable: true, comment: "자동 확정 예정 시각"),
                    confirmed_by_id = table.Column<long>(type: "bigint", nullable: true, comment: "확정자 유형 FK"),
                    cancelled_at = table.Column<DateTime>(type: "datetime", nullable: true, comment: "취소 시각"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    deleted_at = table.Column<DateTime>(type: "timestamp", nullable: true, comment: "Soft Delete 시각")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_transactions_confirmed_by",
                        column: x => x.confirmed_by_id,
                        principalTable: "transaction_confirmed_bys",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_transactions_status",
                        column: x => x.status_id,
                        principalTable: "transaction_statuses",
                        principalColumn: "id");
                },
                comment: "거래 정보 테이블 (하나의 거래에 여러 티켓 항목 가능)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpiryDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsRevoked = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "artist_followers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    artist_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "artist_followers_ibfk_1",
                        column: x => x.artist_id,
                        principalTable: "artists",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    artist_id = table.Column<int>(type: "int", nullable: true),
                    title = table.Column<string>(type: "varchar(255)", nullable: false, comment: "공연/이벤트 제목", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "설명", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    poster_image_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, comment: "포스터 이미지 URL", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    venue_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, comment: "장소명", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    venue_address = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, comment: "장소 주소", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    start_at = table.Column<DateTime>(type: "datetime", nullable: true, comment: "공연 시작 시간"),
                    end_at = table.Column<DateTime>(type: "datetime", nullable: true, comment: "공연 종료 시간"),
                    created_by_admin_id = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'", comment: "활성화 여부"),
                    sort_order = table.Column<int>(type: "int", nullable: false, comment: "정렬 순서"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_events_artist",
                        column: x => x.artist_id,
                        principalTable: "artists",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_events_category",
                        column: x => x.category_id,
                        principalTable: "ticket_category",
                        principalColumn: "id");
                },
                comment: "이벤트/공연 정보 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "chat_rooms",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ticket_id = table.Column<long>(type: "bigint", nullable: false, comment: "티켓 FK"),
                    transaction_id = table.Column<long>(type: "bigint", nullable: true, comment: "거래 FK (거래 성사 시)"),
                    buyer_id = table.Column<long>(type: "bigint", nullable: false, comment: "구매자 FK"),
                    seller_id = table.Column<long>(type: "bigint", nullable: false, comment: "판매자 FK"),
                    status_id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "'1'", comment: "상태 FK"),
                    last_message_at = table.Column<DateTime>(type: "timestamp", nullable: true, comment: "마지막 메시지 시각"),
                    unread_count_buyer = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'", comment: "구매자 읽지 않은 수"),
                    unread_count_seller = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'", comment: "판매자 읽지 않은 수"),
                    locked_at = table.Column<DateTime>(type: "datetime", nullable: true, comment: "채팅 잠금 시각"),
                    closed_at = table.Column<DateTime>(type: "datetime", nullable: true, comment: "채팅 종료 시각"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    deleted_at = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_chat_rooms_status",
                        column: x => x.status_id,
                        principalTable: "chat_room_statuses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_chat_rooms_trans",
                        column: x => x.transaction_id,
                        principalTable: "transactions",
                        principalColumn: "id");
                },
                comment: "채팅방 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "disputes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    transaction_id = table.Column<long>(type: "bigint", nullable: false, comment: "거래 FK"),
                    claimant_id = table.Column<long>(type: "bigint", nullable: false, comment: "신고자 FK"),
                    type_id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "'4'", comment: "분쟁 유형 FK"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "분쟁 내용", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status_id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "'1'", comment: "상태 FK"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_disputes_status",
                        column: x => x.status_id,
                        principalTable: "dispute_statuses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_disputes_trans",
                        column: x => x.transaction_id,
                        principalTable: "transactions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_disputes_type",
                        column: x => x.type_id,
                        principalTable: "dispute_types",
                        principalColumn: "id");
                },
                comment: "분쟁 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "escrow",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    transaction_id = table.Column<long>(type: "bigint", nullable: false, comment: "거래 FK (1:1)"),
                    amount = table.Column<int>(type: "int", nullable: false, comment: "총 금액"),
                    fee_amount = table.Column<int>(type: "int", nullable: false, comment: "수수료"),
                    seller_amount = table.Column<int>(type: "int", nullable: false, comment: "판매자 정산 금액"),
                    status_id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "'1'", comment: "상태 FK"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    released_at = table.Column<DateTime>(type: "datetime", nullable: true, comment: "정산 완료 시각"),
                    refunded_at = table.Column<DateTime>(type: "datetime", nullable: true, comment: "환불 완료 시각"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_escrow_status",
                        column: x => x.status_id,
                        principalTable: "escrow_statuses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_escrow_trans",
                        column: x => x.transaction_id,
                        principalTable: "transactions",
                        principalColumn: "id");
                },
                comment: "에스크로 (결제 대금 보관) 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    transaction_id = table.Column<long>(type: "bigint", nullable: false, comment: "거래 FK"),
                    pg_provider = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, comment: "PG사 (예: toss, kakao)", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_key = table.Column<string>(type: "varchar(255)", nullable: true, comment: "PG사 결제 키", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    order_id = table.Column<string>(type: "varchar(255)", nullable: true, comment: "주문 ID", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    amount = table.Column<int>(type: "int", nullable: false, comment: "결제 금액"),
                    method_id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "'1'", comment: "결제 수단 FK"),
                    paid_at = table.Column<DateTime>(type: "datetime", nullable: true, comment: "결제 완료 시각"),
                    status_id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "'1'", comment: "결제 상태 FK")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_payments_method",
                        column: x => x.method_id,
                        principalTable: "payment_methods",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_payments_status",
                        column: x => x.status_id,
                        principalTable: "payment_statuses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_payments_trans",
                        column: x => x.transaction_id,
                        principalTable: "transactions",
                        principalColumn: "id");
                },
                comment: "결제 정보 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "settlements",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    transaction_id = table.Column<long>(type: "bigint", nullable: false),
                    seller_id = table.Column<long>(type: "bigint", nullable: false, comment: "판매자 FK"),
                    amount = table.Column<int>(type: "int", nullable: false, comment: "총 금액"),
                    fee = table.Column<int>(type: "int", nullable: false, comment: "수수료"),
                    net_amount = table.Column<int>(type: "int", nullable: false, comment: "순 정산 금액"),
                    bank_account_id = table.Column<long>(type: "bigint", nullable: false, comment: "정산 계좌 FK"),
                    status_id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "'1'", comment: "상태 FK"),
                    scheduled_at = table.Column<DateTime>(type: "datetime", nullable: false, comment: "정산 예정 일시"),
                    processed_at = table.Column<DateTime>(type: "datetime", nullable: true, comment: "정산 완료 시각"),
                    failure_reason = table.Column<string>(type: "text", nullable: true, comment: "실패 사유", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    retry_count = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'", comment: "재시도 횟수"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_settlements_bank",
                        column: x => x.bank_account_id,
                        principalTable: "bank_account",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_settlements_status",
                        column: x => x.status_id,
                        principalTable: "settlement_statuses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_settlements_trans",
                        column: x => x.transaction_id,
                        principalTable: "transactions",
                        principalColumn: "id");
                },
                comment: "정산 정보 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "ticket_verification",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    transaction_id = table.Column<long>(type: "bigint", nullable: false, comment: "거래 FK"),
                    method_id = table.Column<long>(type: "bigint", nullable: false, comment: "검증 방법 FK"),
                    raw_data = table.Column<string>(type: "text", nullable: true, comment: "OCR/QR 원본 데이터", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    verification_result = table.Column<bool>(type: "tinyint(1)", nullable: true, comment: "검증 결과"),
                    verified_by = table.Column<long>(type: "bigint", nullable: true, comment: "검증자 FK (수동 검증 시)"),
                    ocr_confidence = table.Column<float>(type: "float", nullable: true, comment: "OCR 신뢰도"),
                    qr_code_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, comment: "QR코드 해시", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ticket_number = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, comment: "티켓 번호", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    verified_at = table.Column<DateTime>(type: "timestamp", nullable: true, comment: "검증 시각")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_ticket_verification_method",
                        column: x => x.method_id,
                        principalTable: "ticket_verification_methods",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_ticket_verification_trans",
                        column: x => x.transaction_id,
                        principalTable: "transactions",
                        principalColumn: "id");
                },
                comment: "티켓 검증 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "transaction_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    transaction_id = table.Column<long>(type: "bigint", nullable: false),
                    old_status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, comment: "이전 상태 코드", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    new_status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, comment: "새 상태 코드", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    changed_by = table.Column<long>(type: "bigint", nullable: true, comment: "변경자 FK"),
                    changed_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_transaction_history_trans",
                        column: x => x.transaction_id,
                        principalTable: "transactions",
                        principalColumn: "id");
                },
                comment: "거래 상태 변경 이력 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "transaction_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    transaction_id = table.Column<long>(type: "bigint", nullable: false, comment: "거래 FK"),
                    ticket_id = table.Column<long>(type: "bigint", nullable: false, comment: "티켓 FK"),
                    quantity = table.Column<int>(type: "int", nullable: false, comment: "구매 수량"),
                    unit_price = table.Column<int>(type: "int", nullable: false, comment: "단가"),
                    total_price = table.Column<int>(type: "int", nullable: false, comment: "소계 (단가 × 수량)"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_trans_items_trans",
                        column: x => x.transaction_id,
                        principalTable: "transactions",
                        principalColumn: "id");
                },
                comment: "거래 항목 테이블 (티켓별 구매 정보)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "user_reputation",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false, comment: "평가 대상 FK"),
                    reviewer_id = table.Column<long>(type: "bigint", nullable: false, comment: "평가자 FK"),
                    transaction_id = table.Column<long>(type: "bigint", nullable: false, comment: "거래 FK"),
                    rating_type_id = table.Column<long>(type: "bigint", nullable: false, comment: "평가 유형 FK"),
                    score = table.Column<int>(type: "int", nullable: false, comment: "점수 (1-5)"),
                    comment = table.Column<string>(type: "text", nullable: true, comment: "리뷰 내용", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_reputation_rating_type",
                        column: x => x.rating_type_id,
                        principalTable: "reputation_rating_types",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_user_reputation_trans",
                        column: x => x.transaction_id,
                        principalTable: "transactions",
                        principalColumn: "id");
                },
                comment: "사용자 평판 (리뷰) 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "tickets",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    seller_id = table.Column<int>(type: "int", nullable: false),
                    event_id = table.Column<int>(type: "int", nullable: true, comment: "공연 FK"),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, comment: "티켓 제목", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    event_datetime = table.Column<DateTime>(type: "datetime", nullable: false, comment: "공연 일시"),
                    seat_info = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, comment: "좌석 정보", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    quantity = table.Column<int>(type: "int", nullable: false, comment: "총 수량"),
                    remaining_quantity = table.Column<int>(type: "int", nullable: false, comment: "남은 수량"),
                    price = table.Column<int>(type: "int", nullable: false, comment: "판매가"),
                    original_price = table.Column<int>(type: "int", nullable: false, comment: "정가"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "상세 설명", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status_id = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    deleted_at = table.Column<DateTime>(type: "timestamp", nullable: true, comment: "Soft Delete 시각"),
                    seat_features = table.Column<string>(type: "json", nullable: true, comment: "좌석 특징 키워드 (JSON 배열)", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_ticket_category",
                        column: x => x.category_id,
                        principalTable: "ticket_category",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_tickets_event",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_tickets_status",
                        column: x => x.status_id,
                        principalTable: "ticket_statuses",
                        principalColumn: "id");
                },
                comment: "티켓 정보 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "chat_messages",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    room_id = table.Column<long>(type: "bigint", nullable: false, comment: "채팅방 FK"),
                    sender_id = table.Column<long>(type: "bigint", nullable: false, comment: "발신자 FK"),
                    message = table.Column<string>(type: "text", nullable: true, comment: "메시지 내용", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, comment: "이미지 URL", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_chat_messages_room",
                        column: x => x.room_id,
                        principalTable: "chat_rooms",
                        principalColumn: "id");
                },
                comment: "채팅 메시지 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "dispute_evidence",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    dispute_id = table.Column<long>(type: "bigint", nullable: false, comment: "분쟁 FK"),
                    image_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, comment: "증거 이미지 URL", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    note = table.Column<string>(type: "text", nullable: true, comment: "설명", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_dispute_evidence_dispute",
                        column: x => x.dispute_id,
                        principalTable: "disputes",
                        principalColumn: "id");
                },
                comment: "분쟁 증거 자료 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "refunds",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    transaction_id = table.Column<long>(type: "bigint", nullable: false),
                    payment_id = table.Column<long>(type: "bigint", nullable: false, comment: "결제 FK"),
                    amount = table.Column<int>(type: "int", nullable: false, comment: "환불 금액"),
                    reason_id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "'1'", comment: "환불 사유 FK"),
                    status_id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "'1'", comment: "상태 FK"),
                    requested_by = table.Column<long>(type: "bigint", nullable: false, comment: "요청자 FK"),
                    approved_by = table.Column<long>(type: "bigint", nullable: true, comment: "승인자 FK"),
                    processed_at = table.Column<DateTime>(type: "datetime", nullable: true, comment: "처리 완료 시각"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_refunds_payment",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_refunds_reason",
                        column: x => x.reason_id,
                        principalTable: "refund_reasons",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_refunds_status",
                        column: x => x.status_id,
                        principalTable: "refund_statuses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_refunds_trans",
                        column: x => x.transaction_id,
                        principalTable: "transactions",
                        principalColumn: "id");
                },
                comment: "환불 정보 테이블")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateIndex(
                name: "uq_admin_action_types_code",
                table: "admin_action_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_admin_actions_action_type_id",
                table: "admin_actions",
                column: "action_type_id");

            migrationBuilder.CreateIndex(
                name: "idx_admin_actions_admin",
                table: "admin_actions",
                columns: new[] { "admin_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_admin_actions_target",
                table: "admin_actions",
                columns: new[] { "target_type_id", "target_id" });

            migrationBuilder.CreateIndex(
                name: "uq_admin_target_types_code",
                table: "admin_target_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_artist_followers_artist",
                table: "artist_followers",
                column: "artist_id");

            migrationBuilder.CreateIndex(
                name: "idx_artist_followers_user",
                table: "artist_followers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uk_artist_user",
                table: "artist_followers",
                columns: new[] { "artist_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_artists_active",
                table: "artists",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_artists_category",
                table: "artists",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "idx_artists_name",
                table: "artists",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "uq_auth_providers_code",
                table: "auth_providers",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_auth_roles_code",
                table: "auth_roles",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_bank_user",
                table: "bank_account",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_bank_verified",
                table: "bank_account",
                columns: new[] { "user_id", "verified" });

            migrationBuilder.CreateIndex(
                name: "idx_msg_created",
                table: "chat_messages",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_msg_room",
                table: "chat_messages",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "idx_msg_room_created",
                table: "chat_messages",
                columns: new[] { "room_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_msg_sender_created",
                table: "chat_messages",
                columns: new[] { "sender_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "uq_chat_room_statuses_code",
                table: "chat_room_statuses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_chat_buyer_last_msg",
                table: "chat_rooms",
                columns: new[] { "buyer_id", "last_message_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_chat_buyer_status",
                table: "chat_rooms",
                columns: new[] { "buyer_id", "status_id" });

            migrationBuilder.CreateIndex(
                name: "idx_chat_not_deleted",
                table: "chat_rooms",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "idx_chat_seller",
                table: "chat_rooms",
                column: "seller_id");

            migrationBuilder.CreateIndex(
                name: "idx_chat_seller_last_msg",
                table: "chat_rooms",
                columns: new[] { "seller_id", "last_message_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_chat_status_id",
                table: "chat_rooms",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "idx_chat_ticket_buyer",
                table: "chat_rooms",
                columns: new[] { "ticket_id", "buyer_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_chat_transaction",
                table: "chat_rooms",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "idx_dispute_evidence_dispute",
                table: "dispute_evidence",
                column: "dispute_id");

            migrationBuilder.CreateIndex(
                name: "uq_dispute_statuses_code",
                table: "dispute_statuses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_dispute_types_code",
                table: "dispute_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_dispute_claimant",
                table: "disputes",
                column: "claimant_id");

            migrationBuilder.CreateIndex(
                name: "idx_dispute_status",
                table: "disputes",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "idx_dispute_trans",
                table: "disputes",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "idx_dispute_type_id",
                table: "disputes",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "idx_escrow_status_id",
                table: "escrow",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "uq_escrow_transaction",
                table: "escrow",
                column: "transaction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_escrow_statuses_code",
                table: "escrow_statuses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "fk_events_admin",
                table: "events",
                column: "created_by_admin_id");

            migrationBuilder.CreateIndex(
                name: "idx_events_artist",
                table: "events",
                column: "artist_id");

            migrationBuilder.CreateIndex(
                name: "idx_events_category_active_sort",
                table: "events",
                columns: new[] { "category_id", "is_active", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "idx_events_start_at",
                table: "events",
                column: "start_at");

            migrationBuilder.CreateIndex(
                name: "idx_events_title",
                table: "events",
                column: "title");

            migrationBuilder.CreateIndex(
                name: "uq_favorite_types_code",
                table: "favorite_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_notification_platforms_code",
                table: "notification_platforms",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_notification_token_platform_id",
                table: "notification_token",
                column: "platform_id");

            migrationBuilder.CreateIndex(
                name: "idx_notification_token_user",
                table: "notification_token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uq_notification_types_code",
                table: "notification_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_noti_created",
                table: "notifications",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_noti_read",
                table: "notifications",
                column: "read_flag");

            migrationBuilder.CreateIndex(
                name: "idx_noti_type",
                table: "notifications",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "idx_noti_user",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_noti_user_created",
                table: "notifications",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_noti_user_type_created",
                table: "notifications",
                columns: new[] { "user_id", "type_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "uq_payment_methods_code",
                table: "payment_methods",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_payment_statuses_code",
                table: "payment_statuses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_payments_key",
                table: "payments",
                column: "payment_key");

            migrationBuilder.CreateIndex(
                name: "idx_payments_method_id",
                table: "payments",
                column: "method_id");

            migrationBuilder.CreateIndex(
                name: "idx_payments_order",
                table: "payments",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "idx_payments_status_id",
                table: "payments",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "idx_payments_trans",
                table: "payments",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "idx_payments_trans_status",
                table: "payments",
                columns: new[] { "transaction_id", "status_id" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "uq_refund_reasons_code",
                table: "refund_reasons",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_refund_statuses_code",
                table: "refund_statuses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_refunds_payment",
                table: "refunds",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "idx_refunds_reason_id",
                table: "refunds",
                column: "reason_id");

            migrationBuilder.CreateIndex(
                name: "idx_refunds_requested_by",
                table: "refunds",
                column: "requested_by");

            migrationBuilder.CreateIndex(
                name: "idx_refunds_status_id",
                table: "refunds",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "idx_refunds_trans",
                table: "refunds",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "idx_refunds_trans_status",
                table: "refunds",
                columns: new[] { "transaction_id", "status_id" });

            migrationBuilder.CreateIndex(
                name: "uq_reputation_rating_types_code",
                table: "reputation_rating_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_settlement_statuses_code",
                table: "settlement_statuses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_settlements_bank",
                table: "settlements",
                column: "bank_account_id");

            migrationBuilder.CreateIndex(
                name: "idx_settlements_failed",
                table: "settlements",
                columns: new[] { "status_id", "retry_count", "scheduled_at" });

            migrationBuilder.CreateIndex(
                name: "idx_settlements_scheduled",
                table: "settlements",
                column: "scheduled_at");

            migrationBuilder.CreateIndex(
                name: "idx_settlements_seller",
                table: "settlements",
                column: "seller_id");

            migrationBuilder.CreateIndex(
                name: "idx_settlements_status",
                table: "settlements",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "idx_settlements_status_scheduled",
                table: "settlements",
                columns: new[] { "status_id", "scheduled_at" });

            migrationBuilder.CreateIndex(
                name: "idx_settlements_trans",
                table: "settlements",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "uq_ticket_category_code",
                table: "ticket_category",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_ticket_img_ticket",
                table: "ticket_images",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "idx_ticket_price_changed_by",
                table: "ticket_price_history",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "idx_ticket_price_ticket",
                table: "ticket_price_history",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "uq_ticket_statuses_code",
                table: "ticket_statuses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "fk_ticket_verification_method",
                table: "ticket_verification",
                column: "method_id");

            migrationBuilder.CreateIndex(
                name: "idx_verify_trans",
                table: "ticket_verification",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "idx_verify_verified_by",
                table: "ticket_verification",
                column: "verified_by");

            migrationBuilder.CreateIndex(
                name: "uq_ticket_verification_trans_method",
                table: "ticket_verification",
                columns: new[] { "transaction_id", "method_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_ticket_verification_methods_code",
                table: "ticket_verification_methods",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_tickets_category_status",
                table: "tickets",
                columns: new[] { "category_id", "status_id" });

            migrationBuilder.CreateIndex(
                name: "idx_tickets_created",
                table: "tickets",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_tickets_event",
                table: "tickets",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "idx_tickets_event_date",
                table: "tickets",
                column: "event_datetime");

            migrationBuilder.CreateIndex(
                name: "idx_tickets_list",
                table: "tickets",
                columns: new[] { "status_id", "event_datetime" });

            migrationBuilder.CreateIndex(
                name: "idx_tickets_not_deleted",
                table: "tickets",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "idx_tickets_remaining_qty",
                table: "tickets",
                column: "remaining_quantity");

            migrationBuilder.CreateIndex(
                name: "idx_tickets_search",
                table: "tickets",
                columns: new[] { "status_id", "event_datetime", "price" });

            migrationBuilder.CreateIndex(
                name: "idx_tickets_seller",
                table: "tickets",
                column: "seller_id");

            migrationBuilder.CreateIndex(
                name: "idx_tickets_status",
                table: "tickets",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "uq_transaction_confirmed_bys_code",
                table: "transaction_confirmed_bys",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_trans_history_trans",
                table: "transaction_history",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "idx_trans_items_ticket",
                table: "transaction_items",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "idx_trans_items_trans",
                table: "transaction_items",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "uq_trans_items_trans_ticket",
                table: "transaction_items",
                columns: new[] { "transaction_id", "ticket_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_transaction_statuses_code",
                table: "transaction_statuses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "fk_transactions_confirmed_by",
                table: "transactions",
                column: "confirmed_by_id");

            migrationBuilder.CreateIndex(
                name: "idx_trans_buyer",
                table: "transactions",
                column: "buyer_id");

            migrationBuilder.CreateIndex(
                name: "idx_trans_buyer_status",
                table: "transactions",
                columns: new[] { "buyer_id", "status_id" });

            migrationBuilder.CreateIndex(
                name: "idx_trans_created",
                table: "transactions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_trans_not_deleted",
                table: "transactions",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "idx_trans_seller",
                table: "transactions",
                column: "seller_id");

            migrationBuilder.CreateIndex(
                name: "idx_trans_seller_status",
                table: "transactions",
                columns: new[] { "seller_id", "status_id" });

            migrationBuilder.CreateIndex(
                name: "idx_trans_status",
                table: "transactions",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_favorites_type_target",
                table: "user_favorites",
                columns: new[] { "favorite_type_id", "target_id" });

            migrationBuilder.CreateIndex(
                name: "idx_user_favorites_user",
                table: "user_favorites",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uk_user_favorite",
                table: "user_favorites",
                columns: new[] { "user_id", "favorite_type_id", "target_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_user_profile_nickname",
                table: "user_profile",
                column: "nickname");

            migrationBuilder.CreateIndex(
                name: "idx_reputation_rating_type_id",
                table: "user_reputation",
                column: "rating_type_id");

            migrationBuilder.CreateIndex(
                name: "idx_reputation_reviewer",
                table: "user_reputation",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "idx_reputation_trans",
                table: "user_reputation",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "idx_reputation_user",
                table: "user_reputation",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_verif_account",
                table: "user_verification",
                column: "account_verified");

            migrationBuilder.CreateIndex(
                name: "idx_verif_all_verified",
                table: "user_verification",
                columns: new[] { "identity_verified", "phone_verified", "account_verified" });

            migrationBuilder.CreateIndex(
                name: "idx_verif_identity",
                table: "user_verification",
                column: "identity_verified");

            migrationBuilder.CreateIndex(
                name: "idx_users_deleted",
                table: "users",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "idx_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_users_provider_id",
                table: "users",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "idx_users_role_id",
                table: "users",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_actions");

            migrationBuilder.DropTable(
                name: "artist_followers");

            migrationBuilder.DropTable(
                name: "chat_messages");

            migrationBuilder.DropTable(
                name: "dispute_evidence");

            migrationBuilder.DropTable(
                name: "escrow");

            migrationBuilder.DropTable(
                name: "notification_token");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "refunds");

            migrationBuilder.DropTable(
                name: "settlements");

            migrationBuilder.DropTable(
                name: "ticket_images");

            migrationBuilder.DropTable(
                name: "ticket_price_history");

            migrationBuilder.DropTable(
                name: "ticket_verification");

            migrationBuilder.DropTable(
                name: "tickets");

            migrationBuilder.DropTable(
                name: "transaction_history");

            migrationBuilder.DropTable(
                name: "transaction_items");

            migrationBuilder.DropTable(
                name: "user_favorites");

            migrationBuilder.DropTable(
                name: "user_profile");

            migrationBuilder.DropTable(
                name: "user_reputation");

            migrationBuilder.DropTable(
                name: "user_verification");

            migrationBuilder.DropTable(
                name: "admin_action_types");

            migrationBuilder.DropTable(
                name: "admin_target_types");

            migrationBuilder.DropTable(
                name: "chat_rooms");

            migrationBuilder.DropTable(
                name: "disputes");

            migrationBuilder.DropTable(
                name: "escrow_statuses");

            migrationBuilder.DropTable(
                name: "notification_platforms");

            migrationBuilder.DropTable(
                name: "notification_types");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "refund_reasons");

            migrationBuilder.DropTable(
                name: "refund_statuses");

            migrationBuilder.DropTable(
                name: "bank_account");

            migrationBuilder.DropTable(
                name: "settlement_statuses");

            migrationBuilder.DropTable(
                name: "ticket_verification_methods");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "ticket_statuses");

            migrationBuilder.DropTable(
                name: "favorite_types");

            migrationBuilder.DropTable(
                name: "reputation_rating_types");

            migrationBuilder.DropTable(
                name: "chat_room_statuses");

            migrationBuilder.DropTable(
                name: "dispute_statuses");

            migrationBuilder.DropTable(
                name: "dispute_types");

            migrationBuilder.DropTable(
                name: "auth_providers");

            migrationBuilder.DropTable(
                name: "auth_roles");

            migrationBuilder.DropTable(
                name: "payment_methods");

            migrationBuilder.DropTable(
                name: "payment_statuses");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "artists");

            migrationBuilder.DropTable(
                name: "transaction_confirmed_bys");

            migrationBuilder.DropTable(
                name: "transaction_statuses");

            migrationBuilder.DropTable(
                name: "ticket_category");
        }
    }
}
