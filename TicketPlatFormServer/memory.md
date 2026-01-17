# 프로젝트 메모리
memory.md 파일 업데이트 주기 : 주간

## 중요 정보
- 프로젝트 시작일: 2025.12.01
- 팀 구성: HyeonSeong
- 주요 마일스톤 기록:

## 주요 결정사항
- [결정 1]
- [결정 2]

## 알아둘 점
- Controller에서 리턴할 때는 항상 `ApiResponse`를 사용할 것.
- 하나의 API 개발이 완료되면 `api_spec` 문서에 해당 기능의 API 스펙을 반드시 작성할 것.
- Repository에서는 쿼리를 작성하는 Queries.cs, 인터페이스를 정의 하는 IRepository.cs, 구현체를 정의 하는 Repository.cs 세 파일로 정의 하며 Dapper 방식으로 작성한다.
- JWT 토큰 정보가 필요한 값들은 ClaimsExtensions 의 GetUserId를 사용할 것.

## 개발 환경 전환 체크리스트 (회사 Mac ↔ 개인 노트북)

### 필수 확인 사항
1. **MySQL 연결 설정**
   - `appsettings.Development.json`의 연결 문자열 비밀번호 확인
   - 회사: `password=stecdev1234!`
   - 개인: `password=1234`
   - 연결 문자열 형식: `Server=127.0.0.1;Port=3306;Database=TicketPlatFormDB;User=root;Password={비밀번호};SslMode=None;AllowPublicKeyRetrieval=True;`

2. **TicketContext.cs 확인**
   - `OnConfiguring` 메서드에 하드코딩된 연결 문자열이 없는지 확인
   - 있다면 반드시 제거 (appsettings.json 설정을 오버라이드함)

3. **.mcp.json MySQL 설정**
   - `mcpServers.mysql.args` 경로 확인
     - 회사: `/Users/{회사계정}/Desktop/workspace/mcp_servers/mysql-mcp-server/server.js`
     - 개인: `/Users/ihyeonseong/Desktop/workspace/mcp_servers/mysql-mcp-server/server.js`
   - `mcpServers.mysql.env.DB_PASSWORD` 확인
     - 회사: `stecdev1234!`
     - 개인: `1234`

4. **MySQL 서버 실행 확인**
   ```bash
   brew services list | grep mysql
   # mysql이 started 상태인지 확인
   ```

5. **환경별 설정 파일**
   - `.gitignore`에 `appsettings.Development.json` 추가 권장
   - `.gitignore`에 `.mcp.json` 추가 권장 (환경별 경로/비밀번호 다름)
   - 각 환경에 맞는 설정 파일 유지