# 프로필 이미지 URL 갱신 API 명세서

## 개요
만료된 Supabase Signed URL을 갱신하기 위한 API입니다.

**기본 URL**: `/api/users`  
**인증**: 필요 (Bearer Token)  
**응답 형식**: JSON (ApiResponse<T>)

---

## API 엔드포인트

### 프로필 이미지 URL 갱신

**Endpoint**: `POST /api/users/profile/image-refresh`

**Headers**:
| 헤더 | 값 | 설명 |
|------|-----|------|
| Authorization | Bearer {accessToken} | 필수 |
| Content-Type | application/json | 필수 |

**Request Body**:
```json
{
  "userId": 15  // 선택 - null이면 본인
}
```

| 필드 | 타입 | 필수 | 설명 |
|------|------|------|------|
| userId | int? | 선택 | 대상 사용자 ID (null 또는 생략 시 본인) |

---

## Response

### 성공 - 이미지 있음 (200)
```json
{
  "message": "이미지 URL 갱신 성공",
  "data": {
    "profileImageUrl": "https://supabase.co/storage/v1/object/sign/..."
  },
  "statusCode": 200,
  "success": true
}
```

### 성공 - 이미지 없음 (200)
```json
{
  "message": "프로필 이미지가 없습니다.",
  "data": {
    "profileImageUrl": null
  },
  "statusCode": 200,
  "success": true
}
```

### 에러 - 사용자 없음 (404)
```json
{
  "message": "사용자를 찾을 수 없습니다.",
  "data": null,
  "statusCode": 404,
  "success": false
}
```

### 에러 - 인증 실패 (401)
응답 바디 없음 (미들웨어 기본 응답)

---

## Response Fields

| 필드 | 타입 | 설명 |
|------|------|------|
| profileImageUrl | string \| null | 새로 발급된 Signed URL (이미지 없으면 null) |

---

## Flutter 사용 예시

### Dio 클라이언트
```dart
/// 프로필 이미지 URL 갱신
/// [userId]가 null이면 본인의 URL 갱신
Future<String?> refreshProfileImageUrl({int? userId}) async {
  final response = await dio.post(
    '/api/users/profile/image-refresh',
    data: userId != null ? {'userId': userId} : {},
  );
  
  if (response.data['success'] == true) {
    return response.data['data']['profileImageUrl'];
  }
  return null;
}
```

### CachedNetworkImage 연동
```dart
CachedNetworkImage(
  imageUrl: profileImageUrl ?? '',
  errorWidget: (context, url, error) {
    // URL 만료 시 갱신 후 재시도
    _refreshAndReload();
    return const CircularProgressIndicator();
  },
)

Future<void> _refreshAndReload() async {
  final newUrl = await refreshProfileImageUrl();
  if (newUrl != null) {
    setState(() => profileImageUrl = newUrl);
  }
}
```

---

## 클라이언트 사용 시나리오

```
1. getMyProfile API 호출 → profileImageUrl 수신
2. CachedNetworkImage로 이미지 로드 시도
3. 이미지 로드 실패 (401/403 - URL 만료)
4. image-refresh API 호출
5. 새 URL로 이미지 다시 로드
```

---

## 참고사항

- Signed URL 기본 만료 시간: **1시간**
- DB에는 Object Key만 저장, API 응답 시 Signed URL 동적 생성
- 본인/타인 프로필 모두 갱신 가능
