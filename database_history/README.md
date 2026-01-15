# Database History

이 디렉토리는 TicketPlatFormDB 데이터베이스의 덤프 파일을 저장합니다.

## 덤프 파일 형식

- **파일명 형식**: `dump_YYYYMMDD_HHMMSS.sql`
- **생성 방법**: `TicketPlatFormServer/dump_database.py` 스크립트 사용

## 덤프 내용

각 덤프 파일에는 다음이 포함됩니다:
- 모든 테이블의 CREATE TABLE 구문 (스키마)
- 모든 테이블의 데이터 (INSERT 구문)
- Foreign key 체크 비활성화/활성화
- UTF-8 인코딩 설정

## 복원 방법

```bash
mysql -h localhost -P 3306 -u root -p TicketPlatFormDB < dump_YYYYMMDD_HHMMSS.sql
```

## 최신 덤프 생성

프로젝트 루트에서:
```bash
cd TicketPlatFormServer
python3 dump_database.py
```

## 주의사항

- 덤프 파일에는 실제 데이터가 포함되어 있으므로 민감한 정보가 포함될 수 있습니다
- Git에 커밋하기 전에 민감한 정보가 없는지 확인하세요
- 대용량 데이터베이스의 경우 덤프 파일 크기가 클 수 있습니다
