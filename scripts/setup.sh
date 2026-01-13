#!/bin/bash

# ============================================
# TicketPlatFormServer 환경 설정 스크립트
# 새로운 맥북에서 실행하세요
# ============================================

set -e

echo "🚀 TicketPlatFormServer 환경 설정을 시작합니다..."
echo ""

# 색상 정의
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 함수: 명령어 존재 확인
check_command() {
    if command -v $1 &> /dev/null; then
        echo -e "${GREEN}✓${NC} $1 설치됨"
        return 0
    else
        echo -e "${RED}✗${NC} $1 미설치"
        return 1
    fi
}

# 함수: 설치 확인
install_if_missing() {
    if ! check_command $1; then
        echo -e "${YELLOW}→${NC} $1 설치 중..."
        brew install $2
    fi
}

echo "📋 1단계: 필수 도구 확인"
echo "-----------------------------------"

# Homebrew 확인
if ! check_command brew; then
    echo -e "${YELLOW}→${NC} Homebrew 설치 중..."
    /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
fi

# .NET SDK 확인
if ! check_command dotnet; then
    echo -e "${YELLOW}→${NC} .NET SDK 설치 중..."
    brew install dotnet
fi

# MySQL 확인
if ! check_command mysql; then
    echo -e "${YELLOW}→${NC} MySQL 설치 중..."
    brew install mysql
fi

# Node.js 확인 (MCP 서버용)
if ! check_command node; then
    echo -e "${YELLOW}→${NC} Node.js 설치 중..."
    brew install node
fi

echo ""
echo "📦 2단계: NuGet 패키지 복원"
echo "-----------------------------------"

# 프로젝트 루트로 이동
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
cd "$PROJECT_ROOT"

dotnet restore
echo -e "${GREEN}✓${NC} NuGet 패키지 복원 완료"

echo ""
echo "🔨 3단계: 프로젝트 빌드"
echo "-----------------------------------"

dotnet build --no-restore
echo -e "${GREEN}✓${NC} 빌드 완료"

echo ""
echo "🗄️ 4단계: 데이터베이스 설정"
echo "-----------------------------------"

# MySQL 서비스 시작
echo "MySQL 서비스 시작 중..."
brew services start mysql 2>/dev/null || true

echo ""
echo -e "${YELLOW}⚠️  수동 작업 필요:${NC}"
echo ""
echo "1. MySQL 비밀번호 설정:"
echo "   mysql_secure_installation"
echo ""
echo "2. 데이터베이스 생성:"
echo "   mysql -u root -p"
echo "   > CREATE DATABASE TicketPlatFormDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
echo "   > exit;"
echo ""
echo "3. 덤프 적용:"
echo "   mysql -u root -p TicketPlatFormDB < TicketPlatFormServer/database_history/TicketPlatFormDB_dump.sql"
echo ""
echo "4. appsettings.json 수정:"
echo "   TicketPlatFormServer/appsettings.json 파일에서"
echo "   Password=YOUR_PASSWORD 부분을 실제 비밀번호로 변경"
echo ""

echo "============================================"
echo -e "${GREEN}✓ 기본 설정 완료!${NC}"
echo "============================================"
echo ""
echo "서버 실행 방법:"
echo "  cd TicketPlatFormServer"
echo "  dotnet run"
echo ""
echo "Swagger UI:"
echo "  http://localhost:5224/swagger"
echo ""
