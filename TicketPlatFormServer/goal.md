# 프로젝트 목표

업데이트 주기: Daily, Task 단위 , Flutter만 고려할 것.

## 완료된 작업

### ✅ 1. 티켓 이미지 배치 업로드 및 Signed URL 재발급 (2026-01-15)

**구현 기능**:
- 티켓 이미지 최대 5개 배치 업로드
- Signed URL 단일/배치 재발급 API
- SellController JWT 인증 버그 수정 (ClaimTypes.NameIdentifier → User.GetUserId())

**API**:
- `POST /api/sell/tickets` - 티켓 등록 (이미지 업로드 포함)
- `POST /api/sell/tickets/images/refresh` - Signed URL 재발급

### ✅ 2. 회원가입 시 사용자 프로필 자동 생성 (2026-01-15)

**구현 기능**:
- User + UserProfile 원자적 트랜잭션 생성
- 랜덤 닉네임 자동 생성 (형용사 + 명사 조합, 900가지)
- 닉네임 중복 방지 로직 (최대 10회 재시도)
- MySQL ExecutionStrategy 적용하여 트랜잭션 충돌 해결

**기본값**:
| 필드 | 값 | 설명 |
|------|-----|------|
| nickname | 랜덤 생성 | "빠른호랑이", "행복한구름" 등 |
| profile_image_url | null | 이미지 없음 |
| bio | null | 자기소개 없음 |
| manner_temperature | 36.5 | 초기 매너 온도 |
| total_trade_count | 0 | 거래 횟수 0 |

### ✅ 3. 사용자 프로필 조회/수정 API (2026-01-15)

**구현 기능**:
- 내 프로필 조회 (JWT 인증)
- 다른 사용자 프로필 조회 (공개 정보)
- 프로필 수정 (닉네임, 자기소개)
- Supabase object key → Signed URL 자동 변환
- 외부 URL은 그대로 반환

**API**:
- `GET /api/users/profile` - 내 프로필 조회
- `GET /api/users/profile/{userId}` - 다른 사용자 프로필 조회
- `PUT /api/users/profile` - 내 프로필 수정

## 다음 작업

### 🎯 우선 1: 프로필 수정 API 확장 (이미지 포함)

**목표**: 프로필 정보 수정 시 이미지 업로드/교체/삭제를 함께 처리

**구현 항목**:
- `PUT /api/users/profile` - 프로필 수정 (닉네임, 자기소개, 프로필 이미지 포함)
  - `multipart/form-data` 지원 (텍스트 필드 + 이미지 파일)
  - `profileImage`가 있으면 Supabase Storage `user-profile-images/{userId}/` 경로에 업로드
  - 기존 이미지가 있으면 교체 전 삭제
  - `removeProfileImage=true`이면 Supabase에서 삭제하고 DB의 `profile_image_url`을 NULL로 업데이트
  - 이미지가 없고 삭제 플래그가 없으면 기존 이미지 유지
  - 응답은 Signed URL 반환 (object key는 DB에 저장)

**예상 파일**:
- `Services/User/IUserService.cs` - UpdateMyProfile 확장 (이미지 처리 포함)
- `Services/FileUpload/IFileUploadService.cs` - UploadUserProfileImage, DeleteUserProfileImage 추가
- `Controllers/UserController.cs` - `PUT /api/users/profile`에 form-data 처리 추가
- `DTO/User/UpdateUserProfileReqDto.cs` - 이미지/삭제 플래그 필드 추가 (필요 시)

### 🎯 우선 2: 티켓 상세 조회 API 개선

**목표**: 티켓 상세 정보 조회 시 모든 이미지와 판매자 정보 제공

**구현 항목**:
- `GET /api/tickets/detail/{ticketId}` - 티켓 상세 조회
  - 티켓의 모든 이미지를 Signed URL로 반환
  - 판매자 프로필 정보 포함 (nickname, manner_temperature)
  - 거래 상태, 가격 정보 포함

### 🎯 우선 3: 채팅 및 거래 기능 개선

**구현 항목**:
- 채팅방 생성 시 티켓 정보 연동
- 거래 완료 후 매너 온도 평가 기능
- 거래 내역 조회 API

## 향후 고려사항

### 기능 개선
- 닉네임 변경 제한 (예: 30일에 1번)
- 프로필 이미지 리사이징 (최대 500x500px)
- 매너 온도 자동 업데이트 로직
- 사용자 신고/차단 기능

### 성능 최적화
- Signed URL 캐싱 (Redis)
- 이미지 CDN 연동
- DB 인덱스 최적화

### 보안 강화
- 파일 업로드 검증 강화
- Rate Limiting 적용
- 민감 정보 암호화
