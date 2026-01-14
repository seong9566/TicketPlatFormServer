# API 리팩토링 요약

## 개요
Path Variable 방식을 Query Parameter 및 Request Body 방식으로 변경하여 클라이언트 개발 편의성을 향상시켰습니다.

**리팩토링 날짜**: 2026-01-14
**버전**: v2.0

---

## 변경 사항 요약

### 📊 통계
- **변경된 Controller**: 2개 (SellController, ChatController)
- **변경된 GET 엔드포인트**: 6개
- **변경된 POST 엔드포인트**: 4개
- **변경된 DELETE 엔드포인트**: 1개
- **총 변경된 엔드포인트**: 11개

---

## 1. SellController (티켓 판매)

### 변경된 엔드포인트

#### 1.1 공연 일정 조회
```diff
- GET /api/sell/events/{eventId}/schedules
+ GET /api/sell/events/schedules?eventId={eventId}
```

**변경 사유**: Path Variable 제거, Query Parameter 사용

**클라이언트 코드 비교**
```dart
// Before
final url = '/api/sell/events/$eventId/schedules';
final response = await get(url);

// After
final url = '/api/sell/events/schedules';
final response = await get(url, queryParameters: {'eventId': eventId});
```

---

#### 1.2 좌석 옵션 조회
```diff
- GET /api/sell/events/{eventId}/seat-options
+ GET /api/sell/events/seat-options?eventId={eventId}
```

**변경 사유**: Path Variable 제거, Query Parameter 사용

---

#### 1.3 티켓 판매 취소
```diff
- DELETE /api/sell/tickets/{ticketId}
+ DELETE /api/sell/tickets?ticketId={ticketId}
```

**변경 사유**: Path Variable 제거, Query Parameter 사용

---

## 2. ChatController (채팅)

### 2.1 GET 엔드포인트 변경

#### 2.1.1 채팅방 상세 조회
```diff
- GET /api/chat/rooms/{roomId}
+ GET /api/chat/rooms/detail?roomId={roomId}
```

**변경 사유**: Path Variable 제거, Query Parameter 사용

---

#### 2.1.2 메시지 목록 조회
```diff
- GET /api/chat/rooms/{roomId}/messages
+ GET /api/chat/messages?roomId={roomId}
```

**변경 사유**: Path Variable 제거, Query Parameter 사용

**클라이언트 코드 비교**
```dart
// Before
final url = '/api/chat/rooms/$roomId/messages';
final response = await get(url, queryParameters: {
  'lastMessageId': lastMessageId,
  'limit': 50
});

// After
final url = '/api/chat/messages';
final response = await get(url, queryParameters: {
  'roomId': roomId,
  'lastMessageId': lastMessageId,
  'limit': 50
});
```

---

#### 2.1.3 메시지 이미지 URL 재발급
```diff
- GET /api/chat/messages/{messageId}/image-url
+ GET /api/chat/messages/image-url?messageId={messageId}
```

**변경 사유**: Path Variable 제거, Query Parameter 사용

---

### 2.2 POST 엔드포인트 변경

#### 2.2.1 메시지 읽음 처리
```diff
- POST /api/chat/rooms/{roomId}/read
+ POST /api/chat/rooms/read
```

**Request Body**
```json
{
  "roomId": 1
}
```

**변경 사유**: Path Variable을 Request Body로 이동

**클라이언트 코드 비교**
```dart
// Before
final url = '/api/chat/rooms/$roomId/read';
await post(url);

// After
final url = '/api/chat/rooms/read';
await post(url, body: {'roomId': roomId});
```

---

#### 2.2.2 결제 요청
```diff
- POST /api/chat/rooms/{roomId}/request-payment
+ POST /api/chat/rooms/request-payment
```

**Request Body**
```json
{
  "roomId": 1,
  "transactionId": 456
}
```

**변경 사유**: Path Variable을 Request Body로 이동

**DTO 변경**
```diff
public class RequestPaymentReqDto
{
+   public long RoomId { get; set; }
    public long TransactionId { get; set; }
}
```

---

#### 2.2.3 구매 확정
```diff
- POST /api/chat/rooms/{roomId}/confirm-purchase
+ POST /api/chat/rooms/confirm-purchase
```

**Request Body**
```json
{
  "roomId": 1,
  "transactionId": 456
}
```

**변경 사유**: Path Variable을 Request Body로 이동

---

#### 2.2.4 거래 취소
```diff
- POST /api/chat/rooms/{roomId}/cancel
+ POST /api/chat/rooms/cancel
```

**Request Body**
```json
{
  "roomId": 1,
  "transactionId": 456,
  "cancelReason": "구매 의사 취소"
}
```

**변경 사유**: Path Variable을 Request Body로 이동

---

## 3. 변경되지 않은 엔드포인트

### 그대로 유지된 API
- ✅ `GET /api/sell/categories` - 이미 Query Parameter 없음
- ✅ `GET /api/sell/events` - 이미 Query Parameter 사용
- ✅ `POST /api/sell/tickets` - multipart/form-data 사용
- ✅ `GET /api/sell/my-tickets` - 이미 Query Parameter 사용
- ✅ `POST /api/chat/rooms` - Request Body 사용
- ✅ `GET /api/chat/rooms` - 이미 Query Parameter 사용
- ✅ `POST /api/chat/messages` - multipart/form-data 사용

---

## 4. 장점

### 4.1 클라이언트 개발 편의성
```dart
// Path Variable 방식의 문제점
final url = '/api/chat/rooms/$roomId/messages';  // String interpolation 필요
final url = baseUrl.replaceFirst('{roomId}', roomId.toString());  // 복잡한 처리

// Query Parameter 방식의 장점
final url = '/api/chat/messages';
final response = await get(url, queryParameters: {'roomId': roomId});  // 간단함
```

### 4.2 일관성 있는 API 디자인
- **GET 요청**: Query Parameter 사용
- **POST/PUT 요청**: Request Body 사용
- **DELETE 요청**: Query Parameter 또는 Request Body 사용

### 4.3 유지보수성 향상
- URL 구조 변경 시 클라이언트 코드 수정 최소화
- Query Parameter는 선택적 파라미터 추가 용이
- Type-safe한 DTO 사용으로 유효성 검증 강화

### 4.4 RESTful 표준 준수
- 리소스 식별은 URL 경로로
- 필터링/검색 조건은 Query Parameter로
- 생성/수정 데이터는 Request Body로

---

## 5. 마이그레이션 가이드

### 5.1 클라이언트 코드 변경

#### Dart/Flutter 예시
```dart
// SellController - 공연 일정 조회
// Before
Future<List<Schedule>> getSchedules(int eventId) async {
  final url = '$baseUrl/api/sell/events/$eventId/schedules';
  final response = await http.get(Uri.parse(url), headers: headers);
  // ...
}

// After
Future<List<Schedule>> getSchedules(int eventId) async {
  final uri = Uri.parse('$baseUrl/api/sell/events/schedules')
    .replace(queryParameters: {'eventId': eventId.toString()});
  final response = await http.get(uri, headers: headers);
  // ...
}
```

```dart
// ChatController - 결제 요청
// Before
Future<void> requestPayment(int roomId, int transactionId) async {
  final url = '$baseUrl/api/chat/rooms/$roomId/request-payment';
  final response = await http.post(
    Uri.parse(url),
    headers: headers,
    body: jsonEncode({'transactionId': transactionId}),
  );
  // ...
}

// After
Future<void> requestPayment(int roomId, int transactionId) async {
  final url = '$baseUrl/api/chat/rooms/request-payment';
  final response = await http.post(
    Uri.parse(url),
    headers: headers,
    body: jsonEncode({
      'roomId': roomId,
      'transactionId': transactionId
    }),
  );
  // ...
}
```

---

## 6. 테스트 체크리스트

### 6.1 SellController
- [ ] GET /api/sell/events/schedules?eventId=1
- [ ] GET /api/sell/events/seat-options?eventId=1
- [ ] DELETE /api/sell/tickets?ticketId=123

### 6.2 ChatController (GET)
- [ ] GET /api/chat/rooms/detail?roomId=1
- [ ] GET /api/chat/messages?roomId=1&lastMessageId=100&limit=50
- [ ] GET /api/chat/messages/image-url?messageId=1

### 6.3 ChatController (POST)
- [ ] POST /api/chat/rooms/read (body: {roomId: 1})
- [ ] POST /api/chat/rooms/request-payment (body: {roomId: 1, transactionId: 456})
- [ ] POST /api/chat/rooms/confirm-purchase (body: {roomId: 1, transactionId: 456})
- [ ] POST /api/chat/rooms/cancel (body: {roomId: 1, transactionId: 456, cancelReason: "..."})

---

## 7. 관련 문서

- [티켓 판매 API 스펙](./sell_ticket_api_spec.md)
- [채팅 API 스펙](./chat_api_spec.md)

---

## 8. 추가 정리 작업

### 8.1 삭제된 파일
- ❌ `Config/AwsS3Settings.cs` - AWS S3 사용하지 않음
- ❌ `Services/Storage/S3StorageUploader.cs` - AWS S3 사용하지 않음

### 8.2 추가된 파일
- ✅ `DTO/Chat/MarkMessagesAsReadReqDto.cs` - 메시지 읽음 처리 요청 DTO

### 8.3 수정된 파일
- ✅ `Controllers/SellController.cs`
- ✅ `Controllers/ChatController.cs`
- ✅ `DTO/Chat/RequestPaymentReqDto.cs`
- ✅ `api_spec/sell_ticket_api_spec.md`
- ✅ `api_spec/chat_api_spec.md` (신규)

---

## 9. Breaking Changes

⚠️ **이 리팩토링은 Breaking Change입니다.**

기존 클라이언트는 API 호출이 실패하므로, 클라이언트 코드를 반드시 업데이트해야 합니다.

### 버전 관리 전략 (제안)
1. **옵션 1**: 새 버전 API 엔드포인트 추가 (예: `/api/v2/...`)
2. **옵션 2**: 기존 엔드포인트 유지 + 새 엔드포인트 추가 (Deprecated 표시)
3. **옵션 3**: 현재처럼 즉시 변경 (클라이언트 동시 업데이트 필요)

**현재 적용된 전략**: 옵션 3 (즉시 변경)

---

**작성일**: 2026-01-14
**작성자**: API 리팩토링 팀
**버전**: v2.0
