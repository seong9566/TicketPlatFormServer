# 회원가입 API 명세서

## 개요
사용자 회원가입을 처리하는 API입니다.

**기본 URL**: `/auth`  
**인증**: 없음  
**응답 형식**: JSON (ApiResponse<T> 래퍼 사용)

---

## API 엔드포인트

### 1. 회원가입

**Endpoint**: `POST /auth/sign`

**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "P@ssw0rd!",
  "phone": "010-1234-5678",
  "role": "user",
  "provider": "email"
}
```

**Request Parameters**:
| 필드 | 타입 | 필수 | 설명 |
|------|------|------|------|
| email | string | O | 이메일 (형식 검증) |
| password | string | O | 비밀번호 (provider가 email이 아닌 경우 저장되지 않음) |
| phone | string | X | 휴대폰 번호 |
| role | string | X | 역할 코드 (기본값: `user`) |
| provider | string | O | 가입 유형 코드 (기본값: `email`) |

**허용 값**:
- provider: `email`, `kakao`, `google`, `apple` (대소문자 무관)
- role: `guest`, `user`, `admin`

**Success Response**:
```json
{
  "message": "회원가입 성공",
  "data": {
    "email": "user@example.com",
    "phone": "010-1234-5678",
    "role": "user",
    "provider": "email"
  },
  "statusCode": 200,
  "success": true
}
```

**Response Fields**:
| 필드 | 타입 | 설명 |
|------|------|------|
| email | string | 사용자 이메일 |
| phone | string | 사용자 전화번호 (없으면 빈 문자열) |
| role | string | 역할 코드 |
| provider | string | 가입 유형 코드 |

**Error Responses**:

1. **이미 가입된 계정**:
```json
{
  "message": "이미 가입된 계정입니다.",
  "data": null,
  "statusCode": 208,
  "success": false
}
```

2. **허용되지 않은 가입 유형**:
```json
{
  "message": "허용되지 않은 가입 유형 입니다.",
  "data": null,
  "statusCode": 400,
  "success": false
}
```

3. **허용되지 않은 역할**:
```json
{
  "message": "허용되지 않은 역할 입니다.",
  "data": null,
  "statusCode": 400,
  "success": false
}
```

4. **요청 유효성 오류 (이메일 형식/필수값 누락)**:
```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["The Email field is not a valid e-mail address."],
    "Password": ["The Password field is required."]
  }
}
```

**HTTP Status Codes**:
- `200 OK`: 성공
- `208 Already Reported`: 이미 가입된 계정
- `400 Bad Request`: 잘못된 입력값 (provider/role/요청 유효성 오류)

**예시 코드 (cURL)**:
```bash
curl -X POST "http://localhost:5000/auth/sign" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "P@ssw0rd!",
    "phone": "010-1234-5678",
    "role": "user",
    "provider": "email"
  }'
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

