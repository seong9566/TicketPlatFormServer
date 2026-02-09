# 프로필 수정 API 요구사항 명세서 & 백엔드 질문

> **날짜**: 2026-02-09  
> **작성자**: Flutter Client Team → Backend Team  
> **목적**: 사용자 프로필(닉네임, 사진) 수정 기능 구현  
> **우선순위**: 🔴 High (블로킹 이슈)

---

## 📋 목차

1. [개요](#개요)
2. [API 엔드포인트](#api-엔드포인트)
3. [요청 명세](#요청-명세)
4. [응답 명세](#응답-명세)
5. [에러 응답](#에러-응답)
6. [비즈니스 로직](#비즈니스-로직)
7. [백엔드 확인 필요 사항 (34개 질문)](#백엔드-확인-필요-사항)

---

## 개요

### 기능 설명
사용자가 자신의 프로필 정보를 수정할 수 있는 API입니다.

### 수정 가능 항목
- **닉네임** (nickname): 2~20자 이내
- **프로필 사진** (profileImage): 이미지 파일 업로드
- **자기소개** (bio): Optional (현재 UI 미구현, 추후 확장 가능)

### 수정 불가 항목
- **이메일** (email): 회원가입 시 고정, 변경 불가

---

## API 엔드포인트

### 기본 정보

| 항목 | 내용 |
|------|------|
| **Method** | `PUT` |
| **Endpoint** | `/api/users/profile` |
| **Content-Type** | `multipart/form-data` |
| **인증** | Required (Bearer Token) |

### URL 예시
```
PUT http://192.168.2.1:5224/api/users/profile
Authorization: Bearer <access_token>
Content-Type: multipart/form-data
```

---

## 요청 명세

### Request Headers
```http
Authorization: Bearer <access_token>
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary...
```

### Request Body (multipart/form-data)

#### 필드 목록

| 필드명 | 타입 | 필수 여부 | 설명 | 제약 조건 |
|--------|------|-----------|------|-----------|
| `nickname` | String | Optional | 변경할 닉네임 | 2~20자 이내 |
| `bio` | String | Optional | 자기소개 (추후 확장) | 200자 이내 |
| `profileImage` | File | Optional | 프로필 이미지 파일 | JPEG, PNG, 최대 5MB |
| `removeProfileImage` | Boolean | Optional | 프로필 이미지 삭제 여부 | `true` 시 이미지 삭제 |

#### 필드 상세

##### 1. **nickname** (String)
- **설명**: 변경할 닉네임
- **제약 조건**:
  - 2~20자 이내
  - 한글, 영문, 숫자, 특수문자 허용 여부는 백엔드 정책에 따름
  - 중복 닉네임 불가 (409 에러 반환)
- **예시**:
  ```
  nickname=티켓마스터
  ```

##### 2. **bio** (String) - Optional
- **설명**: 자기소개 (현재 UI 미구현, API는 지원)
- **제약 조건**: 200자 이내
- **예시**:
  ```
  bio=콘서트를 사랑하는 티켓 수집가입니다!
  ```

##### 3. **profileImage** (File) - Optional
- **설명**: 업로드할 프로필 이미지 파일
- **제약 조건**:
  - **형식**: JPEG, PNG
  - **최대 크기**: 5MB
  - **권장 해상도**: 512x512px (Flutter에서 리사이징 후 전송)
  - **파일명**: 원본 파일명 유지 (예: `profile_123456.jpg`)
- **Flutter 처리**:
  ```dart
  final image = await picker.pickImage(
    source: ImageSource.gallery,
    maxWidth: 512,
    maxHeight: 512,
    imageQuality: 80,  // 압축률 80%
  );
  ```

##### 4. **removeProfileImage** (Boolean) - Optional
- **설명**: 프로필 이미지 삭제 요청
- **값**:
  - `true`: 기존 프로필 이미지 삭제
  - `false` 또는 없음: 유지
- **우선순위**: `removeProfileImage=true`일 경우, `profileImage` 필드는 무시됨

#### 요청 예시

##### 예시 1: 닉네임만 변경
```http
POST /api/users/profile HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW

------WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Disposition: form-data; name="nickname"

티켓마스터
------WebKitFormBoundary7MA4YWxkTrZu0gW--
```

##### 예시 2: 닉네임 + 프로필 사진 변경
```http
POST /api/users/profile HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW

------WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Disposition: form-data; name="nickname"

티켓마스터
------WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Disposition: form-data; name="profileImage"; filename="profile.jpg"
Content-Type: image/jpeg

<binary image data>
------WebKitFormBoundary7MA4YWxkTrZu0gW--
```

##### 예시 3: 프로필 사진만 삭제
```http
POST /api/users/profile HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW

------WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Disposition: form-data; name="removeProfileImage"

true
------WebKitFormBoundary7MA4YWxkTrZu0gW--
```

---

## 응답 명세

### 성공 응답 (200 OK)

#### Response Headers
```http
Content-Type: application/json
```

#### Response Body
```json
{
  "success": true,
  "data": {
    "userId": 123,
    "email": "user@example.com",
    "nickname": "티켓마스터",
    "profileImageUrl": "https://cdn.example.com/profiles/user123_20260209123456.jpg",
    "bio": "콘서트를 사랑하는 티켓 수집가입니다!",
    "mannerTemperature": 36.5,
    "totalTradeCount": 10
  },
  "message": null,
  "timestamp": "2026-02-09T12:34:56.789Z"
}
```

#### 필드 설명

| 필드명 | 타입 | 설명 | Nullable |
|--------|------|------|----------|
| `success` | Boolean | 성공 여부 (`true` 고정) | No |
| `data` | Object | 수정된 프로필 정보 | No |
| `data.userId` | Integer | 사용자 ID | No |
| `data.email` | String | 이메일 (변경 불가) | No |
| `data.nickname` | String | 수정된 닉네임 | No |
| `data.profileImageUrl` | String | 프로필 이미지 URL | Yes |
| `data.bio` | String | 자기소개 | Yes |
| `data.mannerTemperature` | Double | 매너 온도 (기본값: 36.5) | No |
| `data.totalTradeCount` | Integer | 총 거래 횟수 (기본값: 0) | No |
| `message` | String | 성공 메시지 (nullable) | Yes |
| `timestamp` | String | 응답 시각 (ISO 8601) | No |

---

## 에러 응답

### 에러 응답 형식

```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "NICKNAME_ALREADY_EXISTS",
    "message": "이미 사용 중인 닉네임입니다.",
    "details": "닉네임 '티켓마스터'는 다른 사용자가 사용 중입니다."
  },
  "timestamp": "2026-02-09T12:34:56.789Z"
}
```

### 에러 코드 정의

| HTTP 상태 | 에러 코드 | 메시지 | 설명 |
|-----------|-----------|--------|------|
| **400** | `INVALID_NICKNAME` | 닉네임 형식이 올바르지 않습니다. | 2~20자 제약 위반 |
| **400** | `INVALID_IMAGE_FORMAT` | 지원하지 않는 이미지 형식입니다. | JPEG, PNG 외 형식 |
| **400** | `IMAGE_TOO_LARGE` | 이미지 크기가 너무 큽니다. | 5MB 초과 |
| **400** | `INVALID_REQUEST` | 요청 데이터가 올바르지 않습니다. | 필수 필드 누락 등 |
| **401** | `UNAUTHORIZED` | 인증이 필요합니다. | 토큰 없음/만료 |
| **403** | `FORBIDDEN` | 권한이 없습니다. | 다른 사용자 프로필 수정 시도 |
| **409** | `NICKNAME_ALREADY_EXISTS` | 이미 사용 중인 닉네임입니다. | 중복 닉네임 |
| **413** | `PAYLOAD_TOO_LARGE` | 요청 크기가 너무 큽니다. | 전체 요청 크기 초과 |
| **500** | `INTERNAL_SERVER_ERROR` | 서버 오류가 발생했습니다. | 예상치 못한 오류 |
| **503** | `IMAGE_UPLOAD_FAILED` | 이미지 업로드에 실패했습니다. | 스토리지 서비스 오류 |

### 에러 응답 예시

#### 1. 닉네임 중복 (409)
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "NICKNAME_ALREADY_EXISTS",
    "message": "이미 사용 중인 닉네임입니다.",
    "details": "닉네임 '티켓마스터'는 다른 사용자가 사용 중입니다."
  },
  "timestamp": "2026-02-09T12:34:56.789Z"
}
```

#### 2. 닉네임 형식 오류 (400)
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "INVALID_NICKNAME",
    "message": "닉네임은 2~20자 이내로 입력해주세요.",
    "details": "현재 입력된 닉네임: 21자"
  },
  "timestamp": "2026-02-09T12:34:56.789Z"
}
```

#### 3. 이미지 형식 오류 (400)
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "INVALID_IMAGE_FORMAT",
    "message": "지원하지 않는 이미지 형식입니다.",
    "details": "허용된 형식: JPEG, PNG (현재: GIF)"
  },
  "timestamp": "2026-02-09T12:34:56.789Z"
}
```

#### 4. 인증 오류 (401)
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "UNAUTHORIZED",
    "message": "인증이 필요합니다.",
    "details": "토큰이 만료되었습니다. 다시 로그인해주세요."
  },
  "timestamp": "2026-02-09T12:34:56.789Z"
}
```

---

## 비즈니스 로직

### 1. 닉네임 변경
- **중복 검사**: 다른 사용자와 닉네임 중복 불가
- **형식 검증**: 2~20자 이내
- **특수문자 허용 여부**: 백엔드 정책에 따름 (아래 질문 참조)
- **대소문자 구분**: 확인 필요 (아래 질문 참조)

### 2. 프로필 이미지 업로드
- **이미지 처리**:
  1. Flutter에서 512x512px로 리사이징
  2. 80% 품질로 압축
  3. Multipart로 전송
  4. 백엔드에서 CDN/스토리지에 업로드
  5. URL 생성 후 DB 저장
- **기존 이미지 처리**: 새 이미지 업로드 시 기존 이미지 삭제 여부? (아래 질문 참조)
- **파일명 규칙**: 백엔드에서 유니크한 파일명 생성 권장 (아래 질문 참조)

### 3. 프로필 이미지 삭제
- **삭제 요청**: `removeProfileImage=true`
- **결과**: `profileImageUrl`이 `null` 또는 기본 이미지 URL (아래 질문 참조)
- **물리적 삭제**: 스토리지에서 파일 삭제 여부? (아래 질문 참조)

### 4. 부분 업데이트 (Partial Update)
- **변경할 필드만 전송**: 닉네임만 변경 시 `nickname` 필드만 전송
- **미전송 필드**: 기존 값 유지
- **명시적 삭제**: 이미지 삭제는 `removeProfileImage=true` 필수

---

## 백엔드 확인 필요 사항

> **총 34개 질문**을 카테고리별로 정리했습니다.

### 📊 질문 요약

| 카테고리 | 질문 수 | 우선순위 |
|----------|---------|----------|
| API 엔드포인트 & 메서드 | 3 | 🔴 High |
| 닉네임 규칙 | 6 | 🔴 High |
| 이미지 업로드 | 6 | 🔴 High |
| 프로필 이미지 삭제 | 3 | 🟡 Medium |
| 응답 데이터 | 4 | 🔴 High |
| 에러 처리 | 3 | 🟡 Medium |
| 성능 및 제약 | 3 | 🟢 Low |
| 일정 관련 | 3 | 🔴 High |
| 추가 확인 사항 | 3 | 🟢 Low |

---

### 🔴 1. API 엔드포인트 & 메서드 (최우선)

> **현재 구현**: `PUT /api/users/profile`

#### **Q1**: 엔드포인트가 `PUT /api/users/profile`이 맞나요?
- [ ] Yes
- [ ] No, 실제는: `____________________`

#### **Q2**: HTTP 메서드가 `PUT`이 맞나요?
- [ ] Yes
- [ ] No, 실제는: `PATCH` / `POST`

#### **Q3**: `Content-Type`이 `multipart/form-data`가 맞나요?
- [ ] Yes
- [ ] No, 실제는: `application/json` / 기타

---

### 🔴 2. 닉네임 규칙

#### **Q4**: 닉네임 길이 제한은?
- [ ] 2~20자 (클라이언트 가정)
- [ ] 다른 제한: `____________________`

#### **Q5**: 특수문자 허용 여부는?
- [ ] 모두 허용
- [ ] 일부 허용: `____________________` (예: `_`, `-`만)
- [ ] 불허

#### **Q6**: 대소문자 구분 여부는?
예: `User` vs `user`
- [ ] 구분함 (다른 닉네임으로 인정)
- [ ] 구분 안 함 (같은 닉네임으로 처리)

#### **Q7**: 공백 허용 여부는?
예: `티켓 마스터` vs `티켓마스터`
- [ ] 허용 (중간 공백 가능)
- [ ] 불허 (공백 제거)
- [ ] 선후행 공백만 제거

#### **Q8**: 연속된 특수문자 금지 여부는?
예: `user__123`
- [ ] 허용
- [ ] 불허

#### **Q9**: 한글/영문/숫자 외 다른 언어 허용 여부는?
예: 일본어, 중국어, 이모지 등
- [ ] 허용
- [ ] 불허
- [ ] 일부 허용: `____________________`

---

### 🔴 3. 이미지 업로드

#### **Q10**: 허용 이미지 형식은?
- [ ] JPEG, PNG만
- [ ] JPEG, PNG, WebP
- [ ] JPEG, PNG, WebP, HEIF
- [ ] 기타: `____________________`

#### **Q11**: 최대 파일 크기는?
- [ ] 5MB (클라이언트 가정)
- [ ] 다른 크기: `__________MB`

#### **Q12**: 이미지 리사이징 처리는?
- [ ] 클라이언트에서만 (Flutter에서 512x512)
- [ ] 백엔드에서만
- [ ] 클라이언트 + 백엔드 둘 다

#### **Q13**: 파일명 규칙은?
- [ ] 클라이언트 파일명 유지
- [ ] 백엔드에서 재생성 (예: `user123_timestamp.jpg`)
- [ ] UUID 사용

#### **Q14**: 이미지 업로드 스토리지는?
- [ ] AWS S3
- [ ] Azure Blob Storage
- [ ] 자체 서버
- [ ] 기타: `____________________`

#### **Q15**: CDN URL 형식은?
예시를 제공해주세요:
```
____________________
```

---

### 🟡 4. 프로필 이미지 삭제

#### **Q16**: 이미지 삭제 시 `profileImageUrl` 값은?
- [ ] `null`
- [ ] 기본 이미지 URL: `____________________`
- [ ] 빈 문자열 (`""`)

#### **Q17**: 기존 이미지 파일 물리적 삭제 여부는?
- [ ] 즉시 삭제 (스토리지 용량 절약)
- [ ] Soft Delete (일정 기간 보관 후 삭제)
- [ ] 삭제 안 함

#### **Q18**: 새 이미지 업로드 시 기존 이미지 처리는?
- [ ] 자동 삭제
- [ ] 보관 (버전 관리)
- [ ] 클라이언트가 명시적으로 삭제 요청해야 함

---

### 🔴 5. 응답 데이터

#### **Q19**: 응답 형식이 다음과 일치하나요?
```json
{
  "success": true,
  "data": { ... },
  "error": null,
  "timestamp": "2026-02-09T12:34:56.789Z"
}
```
- [ ] Yes
- [ ] No, 실제 형식:
```json
____________________
```

#### **Q20**: `profileImageUrl`이 Signed URL인가요?
- [ ] Yes, Signed URL (일정 시간 후 만료)
- [ ] No, 영구 URL

#### **Q21**: Signed URL일 경우 만료 시간은?
- [ ] N/A (영구 URL)
- [ ] 1시간
- [ ] 24시간
- [ ] 7일
- [ ] 기타: `____________________`

#### **Q22**: `timestamp` 형식은?
- [ ] ISO 8601 (`2026-02-09T12:34:56.789Z`)
- [ ] Unix Timestamp
- [ ] 기타: `____________________`

---

### 🟡 6. 에러 처리

#### **Q23**: 에러 코드 체계는 다음과 일치하나요?
| HTTP | 에러 코드 | 메시지 |
|------|-----------|--------|
| 400 | `INVALID_NICKNAME` | 닉네임 형식이 올바르지 않습니다. |
| 409 | `NICKNAME_ALREADY_EXISTS` | 이미 사용 중인 닉네임입니다. |

- [ ] Yes, 일치
- [ ] No, 실제 에러 코드는: `____________________`

#### **Q24**: 400 에러 시 구체적인 검증 실패 정보를 제공하나요?
예: `"details": "현재 입력된 닉네임: 21자"`
- [ ] Yes, `details` 필드에 제공
- [ ] No, 제공 안 함

#### **Q25**: 409 에러(닉네임 중복) 시 추천 닉네임 제공 가능한가요?
예: `"suggestedNicknames": ["티켓마스터1", "티켓마스터_2"]`
- [ ] Yes, 제공 가능
- [ ] No, 제공 안 함

---

### 🟢 7. 성능 및 제약

#### **Q26**: Rate Limit이 있나요?
- [ ] 없음
- [ ] 있음: `____________________` (예: 1시간에 3번)

#### **Q27**: 이미지 업로드 타임아웃은?
- [ ] 30초
- [ ] 60초
- [ ] 기타: `__________초`

#### **Q28**: Multipart 요청 최대 크기는?
- [ ] 10MB
- [ ] 20MB
- [ ] 기타: `__________MB`

---

### 🔴 8. 일정 관련

#### **Q29**: API 개발 완료 예정일은?
```
____________________
```

#### **Q30**: 테스트 서버 배포 일정은?
```
____________________
```

#### **Q31**: 프로덕션 배포 예정일은?
```
____________________
```

---

### 🟢 9. 추가 확인 사항

#### **Q32**: 프로필 수정 이력 관리 여부는?
- [ ] 이력 관리 함 (감사 로그)
- [ ] 이력 관리 안 함

#### **Q33**: 프로필 수정 알림 발송 여부는?
예: "프로필이 변경되었습니다" 이메일/푸시
- [ ] 알림 발송 함
- [ ] 알림 발송 안 함

#### **Q34**: 닉네임 변경 횟수 제한이 있나요?
- [ ] 없음
- [ ] 있음: `____________________` (예: 월 1회)

---

## ✅ 백엔드팀 체크리스트

작성 완료 후 체크해주세요:

- [ ] 모든 질문에 답변 완료 (34개)
- [ ] API 엔드포인트 확정
- [ ] 에러 코드 문서화 완료
- [ ] Postman Collection 제공 (optional)
- [ ] Swagger/OpenAPI 문서 업데이트 (optional)

---

## 📎 참고 사항

### Flutter Client 구현 상태 (완료)

- ✅ UI: `ProfileEditView` 완성
- ✅ State Management: `ProfileEditViewModel` (Riverpod)
- ✅ DTO: `UpdateProfileReqDto` (multipart/form-data 변환)
- ✅ Repository: `ProfileRepositoryImpl`
- ✅ UseCase: `UpdateMyProfileUsecase`
- ✅ 이미지 선택: `ImagePicker` (512x512, 80% 품질)
- ✅ 에러 처리: 409/400 상태 코드 핸들링

### 테스트 계획

#### 정상 케이스
1. 닉네임만 변경
2. 프로필 사진만 변경
3. 닉네임 + 프로필 사진 동시 변경
4. 프로필 사진 삭제

#### 에러 케이스
1. 닉네임 중복 (409)
2. 닉네임 길이 제약 위반 (400)
3. 이미지 형식 오류 (400)
4. 이미지 크기 초과 (400)
5. 인증 토큰 만료 (401)
6. 권한 없음 (403)

---

## 📧 연락처

**질문 및 피드백**: Flutter Client Team  
**백엔드 담당**: [백엔드팀 담당자명]

---

**답변 기한**: `____________________`  
**우선순위**: 🔴 High (블로킹 이슈)

---

## 문서 버전

| 버전 | 날짜 | 변경 내용 | 작성자 |
|------|------|-----------|--------|
| 1.0 | 2026-02-09 | 초안 작성 | Flutter Team |
| 2.0 | 2026-02-09 | 질문 리스트 통합 (34개) | Flutter Team |

---

**END OF DOCUMENT**
