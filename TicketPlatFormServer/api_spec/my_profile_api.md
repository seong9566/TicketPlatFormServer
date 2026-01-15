# 내 프로필 조회 API 명세서

## 개요
로그인 사용자의 프로필 정보를 조회하는 API입니다.

**기본 URL**: `/api/users`  
**인증**: 필요 (Bearer Token)  
**응답 형식**: JSON (ApiResponse<T> 래퍼 사용)

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
