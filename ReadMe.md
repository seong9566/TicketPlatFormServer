# TicketPlatForm Server

## 🛠 기술 스택 (Tech Stack)

| 구분 | 기술 | 버전      | 설명 |
| --- | --- |---------| --- |
| **Server** | .NET | 9.0.308 | 백엔드 API |
| **Mobile** | Flutter | 3.35.4  | 크로스 플랫폼 모바일 앱 |
| **Database** | MySQL | 9.0     | 관계형 데이터베이스 |
| **IDE** | Cursor, Rider | -       | 개발 환경 |
| **VCS** | GitHub | -       | 버전 관리 |
| **CI/CD** | GitHub Actions | -       | 자동화된 CI/CD 파이프라인 (추가 예정)|

---

## 🚀 환경 설정 가이드 (Quick Start)

### 1. 필수 설치 항목

```bash
# .NET 9 SDK 설치 (Homebrew)
brew install dotnet@9

# MySQL 설치
brew install mysql

# Node.js 설치 (MCP 서버용)
brew install node
```

### 2. 데이터베이스 설정

```bash
# MySQL 서비스 시작
brew services start mysql

# MySQL 접속 (초기 비밀번호 설정)
mysql -u root

# 데이터베이스 생성
CREATE DATABASE TicketPlatFormDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

# 스키마 적용
mysql -u root -p TicketPlatFormDB < DataBase/schema.sql
```

### 3. 프로젝트 설정

```bash
# 프로젝트 클론
git clone <repository-url>
cd TicketPlatFormServer

# NuGet 패키지 복원
dotnet restore

# 빌드
dotnet build
```

### 4. appsettings.json 설정

`TicketPlatFormServer/appsettings.json` 파일에서 DB 연결 정보 수정:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=TicketPlatFormDB;User=root;Password=YOUR_PASSWORD;"
  }
}
```

### 5. 서버 실행

```bash
cd TicketPlatFormServer
dotnet run
```

서버 주소: `http://localhost:5224`
Swagger UI: `http://localhost:5224/swagger`

### 6. Cursor MCP 서버 설정 (선택)

`~/.cursor/mcp.json` 파일 생성/수정:

```json
{
  "mcpServers": {
    "dart": {
      "type": "stdio",
      "command": "dart mcp-server --experimental-mcp-server --force-roots-fallback",
      "env": {},
      "args": []
    },
    "sequential-thinking": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-sequential-thinking"],
      "env": {}
    },
    "mysql": {
      "command": "node",
      "args": ["/path/to/mysql-mcp-server/server.js"],
      "env": {
        "DB_HOST": "localhost",
        "DB_USER": "root",
        "DB_PASSWORD": "YOUR_PASSWORD",
        "DB_NAME": "TicketPlatFormDB"
      }
    }
  }
}
```

---

## 📱 모바일 앱 연동 (로컬 테스트)

로컬에서 실기기 테스트 시:

1. 맥북 IP 확인: `ifconfig | grep "inet " | grep -v 127.0.0.1`
2. API 엔드포인트: `http://<맥북IP>:5224/api/...`

### 주요 API 엔드포인트

| Method | Endpoint | 설명 |
|--------|----------|------|
| GET | `/api/home` | 홈 화면 데이터 |
| GET | `/api/events/category/{categoryId}` | 카테고리별 이벤트 |
| POST | `/api/auth/login` | 로그인 |
| POST | `/api/auth/register` | 회원가입 |

---

## 🗄️ 데이터베이스 관리

### 덤프 (백업)

```bash
# 전체 백업
mysqldump -u root -p TicketPlatFormDB > backup_$(date +%Y%m%d).sql

# 스키마만 백업
mysqldump -u root -p --no-data TicketPlatFormDB > schema_backup.sql

# 데이터만 백업
mysqldump -u root -p --no-create-info TicketPlatFormDB > data_backup.sql
```

### 복원

```bash
mysql -u root -p TicketPlatFormDB < backup.sql
```

### EF Core 스캐폴딩 (DB → 모델 동기화)

```bash
cd TicketPlatFormServer

dotnet ef dbcontext scaffold \
  "Server=localhost;Port=3306;Database=TicketPlatFormDB;User=root;Password=YOUR_PASSWORD;" \
  Pomelo.EntityFrameworkCore.MySql \
  --output-dir DBModel \
  --context TicketContext \
  --context-dir Repository \
  --force
```

---

## 📋 기능 및 명세 (Features & Specifications)

### 1. 회원 정보 (User Management)
*   **가입 및 로그인**:
    *   이메일
    *   소셜 로그인 (Google, Kakao, Apple)
*   **본인 인증**:
    *   단순 조회 시 선택 사항.
    *   **거래 시 필수**: 실명 인증, 휴대폰 인증, 계좌 인증.
*   **유저 프로필**:
    *   닉네임, 이메일(ID), 비밀번호, 휴대폰 번호.
    *   계좌 번호 (선택, 판매 시 필수).
*   **평판 시스템**: 사용자 평점 및 거래 횟수 표시.

### 2. 티켓 정보 (Ticket Information)
*   **종류**: 콘서트, 뮤지컬, 스포츠, 페스티벌 등.
*   **상세 정보**:
    *   공연명, 일시.
    *   좌석 및 구역 정보 (문자열 입력).
    *   티켓 이미지.
    *   판매 수량, 연석 여부.
    *   판매 가격, 원가.
    *   **할인율**: 앱 내에서 원가 대비 판매가를 계산하여 표시.
    *   설명 및 특이사항.

### 3. 거래 정책 (Transaction Policy)
*   **기본 규칙**: 1 티켓 = 1 구매자.
*   **조회**: 다수의 사용자가 동일 티켓 조회 가능.
*   **예약 및 확정**:
    *   구매 요청 시 자동 예약 시간: *미설정*.
    *   구매 확정: 사용자가 수동으로 확정.
    *   자동 확정: *미설정*.

### 4. 결제 (Payment)
*   **PG사**: 토스 페이먼츠 (예정).
*   **결제 수단**: 신용카드, 가상계좌.
*   **플랫폼 수수료**: 5% 미만.

### 5. 보관금 정책 (Escrow Policy)
*   **흐름**: `결제(Payment)` -> `보관(Hold)` -> `검증/전달` -> `지급(Release)`.
*   **상태 정의**:
    *   **HOLD**: 결제 완료, 티켓 전달 대기 중.
    *   **RELEASED**: 거래 완료, 정산 대기 상태.
    *   **FROZEN**: 분쟁 발생, 자금 동결.
    *   **REFUNDED**: 거래 취소, 구매자에게 환불 완료.

### 6. 정산 정책 (Settlement Policy)
*   **필수 조건**: 판매자 실명 인증 및 계좌 인증 완료.
*   **정산 주기**: **D+3** (분쟁 및 클레임 대비를 위한 권장 주기).
*   **재시도 로직**: 정산 실패 시 자동 재시도 (3~5회).
*   **환불 사유**:
    *   QR 검증 실패.
    *   잘못된 티켓 전달.
    *   공연/경기 취소.
    *   사기 의심 신고 (관리자 승인 시).

### 7. 검증 시스템 (Verification System)
*   **방식**:
    *   **QR 코드**: 해시 기반 검증.
    *   **OCR**: 티켓 이미지 광학 문자 인식.
    *   **티켓 번호**: 수동 입력 검증.
    *   **이미지 업로드**: 실물 티켓 소지 인증.

### 8. 신고 및 분쟁 (Disputes & Reporting)
*   **참여자**: 구매자(신고), 판매자(소명), 관리자(판결).
*   **프로세스**:
    1.  **신고 접수**: 구매자 또는 판매자가 문제 제기.
    2.  **심사**: 관리자가 증거(채팅 로그, 이미지 등) 검토.
    3.  **처리**: 승인(환불/페널티) 또는 거절(거래 진행).

### 9. 알림 (Notifications)
*   **발송 시점**:
    *   구매 요청 도착.
    *   결제 완료.
    *   검증 요청.
    *   구매 확정.
    *   정산 완료.
    *   신고(분쟁) 발생.

### 10. 채팅 시스템 (Chat System)
*   **기능**:
    *   구매자-판매자 간 1:1 채팅.
    *   이미지 전송 지원.
    *   **프라이버시**: 분쟁 발생 시에만 관리자 개입/열람 가능.
    *   **구조**: 거래/티켓 당 여러 채팅방 생성 가능.

### 11. 거래 이력 (Transaction History)
*   **기록**: 구매 및 판매 내역.
*   **상태 추적**: 거래 진행 단계 시각화.
*   **피드백**: 거래 완료 후 평판 시스템 연동.