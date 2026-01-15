# 사용자 프로필 API 명세서

## 개요
로그인 사용자의 프로필 정보를 조회하고 수정하는 API입니다.

**기본 URL**: `/api/users`
**인증**: 필요 (Bearer Token)
**응답 형식**: JSON (ApiResponse<T> 래퍼 사용)

## 목차
1. [내 프로필 조회](#1-내-프로필-조회)
2. [내 프로필 수정](#2-내-프로필-수정-닉네임-자기소개-프로필-이미지-포함)
3. [다른 사용자 프로필 조회](#3-다른-사용자-프로필-조회)
4. [공통 응답 구조](#공통-응답-구조)

---

## API 엔드포인트

### 1. 내 프로필 조회

**Endpoint**: `GET /api/users/profile`

**Headers**:
| 헤더 | 값 | 설명 |
|------|-----|------|
| Authorization | Bearer {accessToken} | 필수 |

**Success Response**:
```json
{
  "message": "프로필 조회 성공",
  "data": {
    "userId": 12,
    "email": "user@example.com",
    "nickname": "티켓러버",
    "profileImageUrl": "https://example.com/signed/profile.jpg",
    "bio": "티켓 좋아해요",
    "mannerTemperature": 36.5,
    "totalTradeCount": 0
  },
  "statusCode": 200,
  "success": true
}
```

**Response Fields**:
| 필드 | 타입 | 설명 |
|------|------|------|
| userId | integer | 사용자 ID |
| email | string | 이메일 주소 |
| nickname | string | 닉네임 (미설정 시 빈 문자열) |
| profileImageUrl | string \| null | 프로필 이미지 Signed URL (없으면 null) |
| bio | string \| null | 자기소개 |
| mannerTemperature | number \| null | 매너 온도 |
| totalTradeCount | integer \| null | 총 거래 횟수 |

**Error Responses**:

1. **인증 실패**:
- HTTP 401
- 응답 바디는 기본 인증 미들웨어 응답(본문 없음)일 수 있음

2. **프로필을 찾을 수 없음**:
```json
{
  "message": "프로필을 찾을 수 없습니다.",
  "data": null,
  "statusCode": 404,
  "success": false
}
```

**HTTP Status Codes**:
- `200 OK`: 성공
- `401 Unauthorized`: 인증 실패
- `404 Not Found`: 프로필을 찾을 수 없음

**예시 코드 (cURL)**:
```bash
curl -X GET "http://localhost:5000/api/users/profile" \
  -H "Authorization: Bearer {accessToken}"
```

---

### 2. 내 프로필 수정 (닉네임, 자기소개, 프로필 이미지 포함)

**Endpoint**: `PUT /api/users/profile`

**Headers**:
| 헤더 | 값 | 설명 |
|------|-----|------|
| Authorization | Bearer {accessToken} | 필수 |
| Content-Type | multipart/form-data | 필수 |

**Request Body** (multipart/form-data):
| 필드 | 타입 | 필수 | 설명 |
|------|------|------|------|
| nickname | string | 선택 | 닉네임 (최대 50자, 중복 불가) |
| bio | string | 선택 | 자기소개 (최대 500자) |
| profileImage | file | 선택 | 프로필 이미지 파일 (jpg, jpeg, png, gif, webp) |
| removeProfileImage | boolean | 선택 | 프로필 이미지 삭제 플래그 (기본값: false) |

**중요 사항**:
- 모든 필드는 선택 사항이며, 변경하려는 필드만 전송
- 닉네임 검증: 최대 50자, 중복 불가
- 자기소개 검증: 최대 500자
- **이미지 삭제 우선순위**: `removeProfileImage=true`이면 `profileImage`가 함께 전송되더라도 삭제가 우선됨
- 이미지 교체: 기존 이미지가 있으면 삭제 후 업로드
- 이미지는 Supabase Storage의 `user-profile-images/{userId}/` 경로에 저장
- 업로드된 이미지는 object key로 저장되며, 응답에는 Signed URL로 반환

**Success Response**:
```json
{
  "message": "프로필 수정 성공",
  "data": {
    "userId": 12,
    "email": "user@example.com",
    "nickname": "새로운닉네임",
    "profileImageUrl": "https://supabase.storage/signed/user-profile-images/12/abc123.jpg?token=xyz",
    "bio": "새로운 자기소개",
    "mannerTemperature": 36.5,
    "totalTradeCount": 0
  },
  "statusCode": 200,
  "success": true
}
```

**Error Responses**:

1. **인증 실패**:
- HTTP 401
- 응답 바디는 기본 인증 미들웨어 응답일 수 있음

2. **닉네임 중복**:
```json
{
  "message": "이미 사용 중인 닉네임입니다.",
  "data": null,
  "statusCode": 409,
  "success": false
}
```

3. **길이 검증 실패**:
```json
{
  "message": "닉네임은 최대 50자까지 입력 가능합니다.",
  "data": null,
  "statusCode": 400,
  "success": false
}
```

```json
{
  "message": "자기소개는 최대 500자까지 입력 가능합니다.",
  "data": null,
  "statusCode": 400,
  "success": false
}
```

4. **프로필을 찾을 수 없음**:
```json
{
  "message": "프로필을 찾을 수 없습니다.",
  "data": null,
  "statusCode": 404,
  "success": false
}
```

5. **파일 검증 실패**:
```json
{
  "message": "파일 크기는 10MB를 초과할 수 없습니다.",
  "data": null,
  "statusCode": 400,
  "success": false
}
```

```json
{
  "message": "허용되지 않는 파일 형식입니다. 허용된 형식: .jpg, .jpeg, .png, .gif, .webp",
  "data": null,
  "statusCode": 400,
  "success": false
}
```

```json
{
  "message": "파일 내용이 확장자와 일치하지 않습니다.",
  "data": null,
  "statusCode": 400,
  "success": false
}
```

**HTTP Status Codes**:
- `200 OK`: 성공
- `400 Bad Request`: 파일 검증 실패 (크기, 형식, Magic bytes)
- `401 Unauthorized`: 인증 실패
- `404 Not Found`: 프로필을 찾을 수 없음
- `409 Conflict`: 닉네임 중복

**사용 예시**:

1. **닉네임만 변경**:
```bash
curl -X PUT "http://localhost:5000/api/users/profile" \
  -H "Authorization: Bearer {accessToken}" \
  -F "nickname=새로운닉네임"
```

2. **닉네임과 자기소개 변경**:
```bash
curl -X PUT "http://localhost:5000/api/users/profile" \
  -H "Authorization: Bearer {accessToken}" \
  -F "nickname=새닉네임" \
  -F "bio=새로운 자기소개입니다"
```

3. **프로필 이미지 업로드 (기존 이미지 자동 교체)**:
```bash
curl -X PUT "http://localhost:5000/api/users/profile" \
  -H "Authorization: Bearer {accessToken}" \
  -F "profileImage=@/path/to/image.jpg"
```

4. **닉네임과 프로필 이미지 함께 변경**:
```bash
curl -X PUT "http://localhost:5000/api/users/profile" \
  -H "Authorization: Bearer {accessToken}" \
  -F "nickname=새닉네임" \
  -F "bio=안녕하세요" \
  -F "profileImage=@/path/to/image.jpg"
```

5. **프로필 이미지 삭제**:
```bash
curl -X PUT "http://localhost:5000/api/users/profile" \
  -H "Authorization: Bearer {accessToken}" \
  -F "removeProfileImage=true"
```


---

### 3. 다른 사용자 프로필 조회

**Endpoint**: `GET /api/users/profile/{userId}?userId={userId}`

**Headers**:
| 헤더 | 값 | 설명 |
|------|-----|------|
| Authorization | Bearer {accessToken} | 필수 |

**Path Parameters**:
| 파라미터 | 타입 | 필수 | 설명 |
|---------|------|------|------|
| userId | integer | 필수 | 조회할 사용자 ID |

**Query Parameters**:
| 파라미터 | 타입 | 필수 | 설명 |
|---------|------|------|------|
| userId | integer | 필수 | 서버가 사용하는 사용자 ID (Path의 userId와 동일 값 권장) |

**Success Response**:
```json
{
  "message": "프로필 조회 성공",
  "data": {
    "userId": 15,
    "email": "other@example.com",
    "nickname": "다른사용자",
    "profileImageUrl": "https://example.com/signed/profile.jpg",
    "bio": "안녕하세요",
    "mannerTemperature": 38.2,
    "totalTradeCount": 5
  },
  "statusCode": 200,
  "success": true
}
```

**Response Fields**:
프로필 조회 API와 동일 (공개 정보이므로 모든 필드 반환)

**Error Responses**:

1. **인증 실패**:
- HTTP 401

2. **사용자를 찾을 수 없음**:
```json
{
  "message": "사용자를 찾을 수 없습니다.",
  "data": null,
  "statusCode": 404,
  "success": false
}
```

**HTTP Status Codes**:
- `200 OK`: 성공
- `401 Unauthorized`: 인증 실패
- `404 Not Found`: 사용자를 찾을 수 없음

**예시 코드 (cURL)**:
```bash
curl -X GET "http://localhost:5000/api/users/profile/15?userId=15" \
  -H "Authorization: Bearer {accessToken}"
```

---

## 공통 응답 구조

모든 API는 다음과 같은 통일된 응답 구조를 사용합니다:

```json
{
  "message": "응답 메시지",
  "data": { ... },
  "statusCode": 200,
  "success": true
}
```

**공통 필드**:
| 필드 | 타입 | 설명 |
|------|------|------|
| message | string | 응답 메시지 (한글) |
| data | object \| array \| null | 응답 데이터 |
| statusCode | integer | HTTP 상태 코드 |
| success | boolean | 성공 여부 (200-299: true, 그 외: false) |
