#!/bin/bash
# TicketPlatFormDB 복원 스크립트
# 사용법: ./db_restore.sh [호스트] [포트] [사용자]

HOST=${1:-localhost}
PORT=${2:-3306}
USER=${3:-root}

echo "=== TicketPlatFormDB 복원 ==="
echo "호스트: $HOST:$PORT"
echo "사용자: $USER"
echo ""

read -sp "MySQL 비밀번호 입력: " PASSWORD
echo ""

echo "DB 복원 중..."
mysql -h "$HOST" -P "$PORT" -u "$USER" -p"$PASSWORD" < TicketPlatFormDB_dump.sql

if [ $? -eq 0 ]; then
    echo "복원 완료!"
else
    echo "복원 실패. 오류를 확인하세요."
    exit 1
fi
