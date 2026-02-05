#!/bin/bash
# TicketPlatFormDB 덤프 스크립트
# 사용법: ./db_dump.sh [사용자] [비밀번호]

USER=${1:-root}
PASSWORD=${2}
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
DUMP_FILE="past_dump/TicketPlatFormDB_dump_${TIMESTAMP}.sql"

echo "=== TicketPlatFormDB 덤프 ==="
echo "사용자: $USER"
echo "출력: $DUMP_FILE"
echo ""

# 비밀번호가 인자로 제공되지 않으면 프롬프트
if [ -z "$PASSWORD" ]; then
    read -sp "MySQL 비밀번호 입력: " PASSWORD
    echo ""
fi

echo "DB 덤프 중..."
mysqldump -u "$USER" -p"$PASSWORD" \
  --single-transaction \
  --routines \
  --triggers \
  --events \
  TicketPlatFormDB > "$DUMP_FILE" 2>&1

if [ $? -eq 0 ]; then
    FILE_SIZE=$(du -h "$DUMP_FILE" | cut -f1)
    echo "✅ 덤프 완료! (크기: $FILE_SIZE)"
    echo "파일: $DUMP_FILE"
    
    # 최신 덤프를 TicketPlatFormDB_dump.sql로 복사
    echo ""
    echo "최신 덤프를 TicketPlatFormDB_dump.sql로 업데이트..."
    cp "$DUMP_FILE" TicketPlatFormDB_dump.sql
    echo "✅ 업데이트 완료!"
else
    echo "❌ 덤프 실패. 오류를 확인하세요."
    cat "$DUMP_FILE"
    rm -f "$DUMP_FILE"
    exit 1
fi
