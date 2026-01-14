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
