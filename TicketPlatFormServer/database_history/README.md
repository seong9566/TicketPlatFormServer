# Database History

이 디렉터리는 DB 덤프 히스토리(스키마 + 데이터)를 보관합니다.

## 구성
- TicketPlatFormDB_dump.sql: 최신 전체 덤프
- db_restore.sh / db_restore.bat: 복원용 스크립트

## 사용 방법 (덤프 기반)
1. 스키마/데이터 변경 후, 이 디렉터리에 새로운 덤프 파일을 추가합니다.
2. 필요 시 TicketPlatFormDB_dump.sql을 최신 스냅샷으로 유지합니다.
3. 복원 후 스캐폴딩으로 모델을 갱신합니다:

   dotnet ef dbcontext scaffold \
     "Server=localhost;Port=3306;Database=TicketPlatFormDB;User=root;Password=YOUR_PASSWORD;" \
     Pomelo.EntityFrameworkCore.MySql \
     --output-dir DBModel \
     --context TicketContext \
     --context-dir Repository \
     --force

## 참고
- 특별한 목적이 없다면 스키마와 데이터를 함께 포함한 덤프를 사용합니다.
