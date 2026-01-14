# API 변경사항 - 프론트엔드 마이그레이션 가이드

## 📋 문서 정보
- **버전**: v2.0
- **작성일**: 2026-01-14
- **적용 대상**: 모든 프론트엔드 클라이언트
- **중요도**: ⚠️ BREAKING CHANGES

---

## 🚨 중요 공지

**이번 업데이트는 Breaking Change입니다.**

기존 API 호출 방식이 변경되어 클라이언트 코드를 반드시 수정해야 합니다.

### 변경 이유
- 클라이언트 개발 편의성 향상
- URL String Interpolation 제거
- 일관성 있는 API 디자인 (GET: Query Parameter, POST: Request Body)
- 타입 안정성 강화

---

## 📊 변경 요약

### 통계
- **변경된 Controller**: 2개 (SellController, ChatController)
- **변경된 엔드포인트**: 총 11개
  - GET: 6개
  - POST: 4개
  - DELETE: 1개

---

## 🎫 1. SellController (티켓 판매)

### 1.1 공연 일정 조회

#### ❌ Before
```http
GET /api/sell/events/{eventId}/schedules
Authorization: Bearer {token}
```

#### ✅ After
```http
GET /api/sell/events/schedules?eventId={eventId}
Authorization: Bearer {token}
```

#### JavaScript/TypeScript 예시
```typescript
// Before
async function getSchedules(eventId: number) {
  const response = await fetch(`${API_BASE_URL}/api/sell/events/${eventId}/schedules`, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  return await response.json();
}

// After
async function getSchedules(eventId: number) {
  const params = new URLSearchParams({ eventId: eventId.toString() });
  const response = await fetch(`${API_BASE_URL}/api/sell/events/schedules?${params}`, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  return await response.json();
}
```

#### Dart/Flutter 예시
```dart
// Before
Future<List<Schedule>> getSchedules(int eventId) async {
  final url = Uri.parse('$baseUrl/api/sell/events/$eventId/schedules');
  final response = await http.get(url, headers: headers);
  // ...
}

// After
Future<List<Schedule>> getSchedules(int eventId) async {
  final url = Uri.parse('$baseUrl/api/sell/events/schedules')
    .replace(queryParameters: {'eventId': eventId.toString()});
  final response = await http.get(url, headers: headers);
  // ...
}
```

#### Response (변경 없음)
```json
{
  "message": "일정 조회 성공",
  "data": [
    {
      "scheduleId": 1,
      "eventDate": "2026-03-15T19:00:00",
      "venue": "올림픽공원 체조경기장",
      "availableSeats": 500
    }
  ],
  "statusCode": 200
}
```

---

### 1.2 좌석 옵션 조회

#### ❌ Before
```http
GET /api/sell/events/{eventId}/seat-options
```

#### ✅ After
```http
GET /api/sell/events/seat-options?eventId={eventId}
```

#### Axios 예시
```typescript
// Before
const response = await axios.get(`/api/sell/events/${eventId}/seat-options`, {
  headers: { Authorization: `Bearer ${token}` }
});

// After
const response = await axios.get('/api/sell/events/seat-options', {
  params: { eventId },
  headers: { Authorization: `Bearer ${token}` }
});
```

---

### 1.3 티켓 판매 취소

#### ❌ Before
```http
DELETE /api/sell/tickets/{ticketId}
```

#### ✅ After
```http
DELETE /api/sell/tickets?ticketId={ticketId}
```

#### Fetch API 예시
```typescript
// Before
await fetch(`${API_BASE_URL}/api/sell/tickets/${ticketId}`, {
  method: 'DELETE',
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

// After
const params = new URLSearchParams({ ticketId: ticketId.toString() });
await fetch(`${API_BASE_URL}/api/sell/tickets?${params}`, {
  method: 'DELETE',
  headers: {
    'Authorization': `Bearer ${token}`
  }
});
```

---

## 💬 2. ChatController (채팅)

### 2.1 채팅방 상세 조회

#### ❌ Before
```http
GET /api/chat/rooms/{roomId}
```

#### ✅ After
```http
GET /api/chat/rooms/detail?roomId={roomId}
```

#### React Query 예시
```typescript
// Before
const { data } = useQuery(['chatRoom', roomId], () =>
  fetch(`${API_BASE_URL}/api/chat/rooms/${roomId}`, {
    headers: { Authorization: `Bearer ${token}` }
  }).then(res => res.json())
);

// After
const { data } = useQuery(['chatRoom', roomId], () => {
  const params = new URLSearchParams({ roomId: roomId.toString() });
  return fetch(`${API_BASE_URL}/api/chat/rooms/detail?${params}`, {
    headers: { Authorization: `Bearer ${token}` }
  }).then(res => res.json());
});
```

#### Response
```json
{
  "message": "채팅방 상세 조회 성공",
  "data": {
    "roomId": 1,
    "ticketId": 123,
    "ticketTitle": "아이유 콘서트 티켓",
    "ticketPrice": 150000,
    "sellerId": 10,
    "sellerNickname": "판매자",
    "buyerId": 20,
    "buyerNickname": "구매자",
    "status": "active"
  },
  "statusCode": 200
}
```

---

### 2.2 메시지 목록 조회

#### ❌ Before
```http
GET /api/chat/rooms/{roomId}/messages?lastMessageId={lastMessageId}&limit={limit}
```

#### ✅ After
```http
GET /api/chat/messages?roomId={roomId}&lastMessageId={lastMessageId}&limit={limit}
```

#### 중요 변경사항
`roomId`가 **Path Variable**에서 **Query Parameter**로 이동했습니다.

#### Axios 예시
```typescript
// Before
const response = await axios.get(`/api/chat/rooms/${roomId}/messages`, {
  params: { lastMessageId, limit: 50 },
  headers: { Authorization: `Bearer ${token}` }
});

// After
const response = await axios.get('/api/chat/messages', {
  params: { roomId, lastMessageId, limit: 50 },
  headers: { Authorization: `Bearer ${token}` }
});
```

---

### 2.3 메시지 이미지 URL 재발급

#### ❌ Before
```http
GET /api/chat/messages/{messageId}/image-url
```

#### ✅ After
```http
GET /api/chat/messages/image-url?messageId={messageId}
```

---

### 2.4 메시지 읽음 처리

#### ❌ Before
```http
POST /api/chat/rooms/{roomId}/read
Authorization: Bearer {token}
```
(Request Body 없음)

#### ✅ After
```http
POST /api/chat/rooms/read
Authorization: Bearer {token}
Content-Type: application/json

{
  "roomId": 1
}
```

#### 중요 변경사항
- Path Variable → Request Body로 변경
- **Request Body가 필수**로 변경됨

#### Fetch API 예시
```typescript
// Before
await fetch(`${API_BASE_URL}/api/chat/rooms/${roomId}/read`, {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

// After
await fetch(`${API_BASE_URL}/api/chat/rooms/read`, {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({ roomId })
});
```

#### Dart/Flutter 예시
```dart
// Before
await http.post(
  Uri.parse('$baseUrl/api/chat/rooms/$roomId/read'),
  headers: headers,
);

// After
await http.post(
  Uri.parse('$baseUrl/api/chat/rooms/read'),
  headers: {...headers, 'Content-Type': 'application/json'},
  body: jsonEncode({'roomId': roomId}),
);
```

---

### 2.5 결제 요청

#### ❌ Before
```http
POST /api/chat/rooms/{roomId}/request-payment
Content-Type: application/json

{
  "transactionId": 456
}
```

#### ✅ After
```http
POST /api/chat/rooms/request-payment
Content-Type: application/json

{
  "roomId": 1,
  "transactionId": 456
}
```

#### 중요 변경사항
- Path Variable → Request Body로 변경
- Request Body에 **roomId 필드 추가** 필요

#### Axios 예시
```typescript
// Before
await axios.post(`/api/chat/rooms/${roomId}/request-payment`, {
  transactionId
}, {
  headers: { Authorization: `Bearer ${token}` }
});

// After
await axios.post('/api/chat/rooms/request-payment', {
  roomId,
  transactionId
}, {
  headers: { Authorization: `Bearer ${token}` }
});
```

#### Response
```json
{
  "message": "결제 요청이 전송되었습니다",
  "data": {
    "paymentUrl": "https://payment.example.com/...",
    "transactionId": 456
  },
  "statusCode": 200
}
```

---

### 2.6 구매 확정

#### ❌ Before
```http
POST /api/chat/rooms/{roomId}/confirm-purchase
Content-Type: application/json

{
  "transactionId": 456
}
```

#### ✅ After
```http
POST /api/chat/rooms/confirm-purchase
Content-Type: application/json

{
  "roomId": 1,
  "transactionId": 456
}
```

#### JavaScript/TypeScript 예시
```typescript
// Before
async function confirmPurchase(roomId: number, transactionId: number) {
  const response = await fetch(`${API_BASE_URL}/api/chat/rooms/${roomId}/confirm-purchase`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ transactionId })
  });
  return await response.json();
}

// After
async function confirmPurchase(roomId: number, transactionId: number) {
  const response = await fetch(`${API_BASE_URL}/api/chat/rooms/confirm-purchase`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ roomId, transactionId })
  });
  return await response.json();
}
```

---

### 2.7 거래 취소

#### ❌ Before
```http
POST /api/chat/rooms/{roomId}/cancel
Content-Type: application/json

{
  "transactionId": 456,
  "cancelReason": "구매 의사 취소"
}
```

#### ✅ After
```http
POST /api/chat/rooms/cancel
Content-Type: application/json

{
  "roomId": 1,
  "transactionId": 456,
  "cancelReason": "구매 의사 취소"
}
```

---

## 🔧 마이그레이션 체크리스트

### SellController
- [ ] `GET /api/sell/events/schedules` - Query Parameter 사용으로 변경
- [ ] `GET /api/sell/events/seat-options` - Query Parameter 사용으로 변경
- [ ] `DELETE /api/sell/tickets` - Query Parameter 사용으로 변경

### ChatController - GET 엔드포인트
- [ ] `GET /api/chat/rooms/detail` - Query Parameter 사용으로 변경
- [ ] `GET /api/chat/messages` - roomId를 Query Parameter로 추가
- [ ] `GET /api/chat/messages/image-url` - Query Parameter 사용으로 변경

### ChatController - POST 엔드포인트
- [ ] `POST /api/chat/rooms/read` - Request Body에 roomId 추가
- [ ] `POST /api/chat/rooms/request-payment` - Request Body에 roomId 추가
- [ ] `POST /api/chat/rooms/confirm-purchase` - Request Body에 roomId 추가
- [ ] `POST /api/chat/rooms/cancel` - Request Body에 roomId 추가

---

## 🧪 테스트 가이드

### 1. 로컬 테스트 환경
```
Base URL: http://localhost:5224
```

### 2. 인증 토큰 획득
```typescript
// 로그인 후 토큰 저장
const loginResponse = await fetch(`${API_BASE_URL}/api/auth/login`, {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email, password })
});
const { token } = await loginResponse.json();
localStorage.setItem('authToken', token);
```

### 3. API 테스트 스크립트

#### SellController 테스트
```typescript
// 공연 일정 조회
const testEventSchedules = async () => {
  const eventId = 1;
  const params = new URLSearchParams({ eventId: eventId.toString() });
  const response = await fetch(`${API_BASE_URL}/api/sell/events/schedules?${params}`, {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  console.log('일정 조회:', await response.json());
};

// 좌석 옵션 조회
const testSeatOptions = async () => {
  const eventId = 1;
  const params = new URLSearchParams({ eventId: eventId.toString() });
  const response = await fetch(`${API_BASE_URL}/api/sell/events/seat-options?${params}`, {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  console.log('좌석 옵션:', await response.json());
};
```

#### ChatController 테스트
```typescript
// 채팅방 상세 조회
const testChatRoomDetail = async () => {
  const roomId = 1;
  const params = new URLSearchParams({ roomId: roomId.toString() });
  const response = await fetch(`${API_BASE_URL}/api/chat/rooms/detail?${params}`, {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  console.log('채팅방 상세:', await response.json());
};

// 메시지 읽음 처리
const testMarkAsRead = async () => {
  const roomId = 1;
  const response = await fetch(`${API_BASE_URL}/api/chat/rooms/read`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ roomId })
  });
  console.log('읽음 처리:', await response.json());
};

// 결제 요청
const testRequestPayment = async () => {
  const roomId = 1;
  const transactionId = 456;
  const response = await fetch(`${API_BASE_URL}/api/chat/rooms/request-payment`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ roomId, transactionId })
  });
  console.log('결제 요청:', await response.json());
};
```

---

## ⚠️ 주의사항

### 1. Content-Type 헤더
POST 요청 시 반드시 `Content-Type: application/json` 헤더를 포함해야 합니다.

```typescript
// ❌ 잘못된 예시
fetch('/api/chat/rooms/read', {
  method: 'POST',
  body: JSON.stringify({ roomId: 1 })
});

// ✅ 올바른 예시
fetch('/api/chat/rooms/read', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ roomId: 1 })
});
```

### 2. Query Parameter 인코딩
특수 문자가 포함된 경우 URL 인코딩이 필요합니다.

```typescript
// URLSearchParams 사용 (자동 인코딩)
const params = new URLSearchParams({
  eventId: eventId.toString(),
  filter: '특수문자 포함'
});
```

### 3. 타입 안정성
TypeScript를 사용하는 경우 DTO 인터페이스를 정의하세요.

```typescript
interface MarkMessagesAsReadRequest {
  roomId: number;
}

interface RequestPaymentRequest {
  roomId: number;
  transactionId: number;
}

interface ConfirmPurchaseRequest {
  roomId: number;
  transactionId: number;
}

interface CancelTransactionRequest {
  roomId: number;
  transactionId: number;
  cancelReason: string;
}
```

### 4. 에러 처리
기존과 동일한 에러 응답 형식을 사용합니다.

```json
{
  "message": "에러 메시지",
  "data": null,
  "statusCode": 400
}
```

HTTP 상태 코드:
- `400`: Bad Request (잘못된 요청)
- `401`: Unauthorized (인증 필요)
- `403`: Forbidden (권한 없음)
- `404`: Not Found (리소스 없음)
- `500`: Internal Server Error (서버 오류)

---

## 📦 Helper 함수 예시

### TypeScript/JavaScript

```typescript
// API 클라이언트 헬퍼
class ApiClient {
  constructor(private baseUrl: string, private token: string) {}

  // GET with Query Parameters
  async get(endpoint: string, params?: Record<string, any>) {
    const url = new URL(endpoint, this.baseUrl);
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        url.searchParams.append(key, String(value));
      });
    }

    const response = await fetch(url.toString(), {
      headers: {
        'Authorization': `Bearer ${this.token}`
      }
    });

    if (!response.ok) {
      throw new Error(`API Error: ${response.status}`);
    }

    return await response.json();
  }

  // POST with JSON Body
  async post(endpoint: string, body: Record<string, any>) {
    const url = new URL(endpoint, this.baseUrl);

    const response = await fetch(url.toString(), {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(body)
    });

    if (!response.ok) {
      throw new Error(`API Error: ${response.status}`);
    }

    return await response.json();
  }

  // DELETE with Query Parameters
  async delete(endpoint: string, params?: Record<string, any>) {
    const url = new URL(endpoint, this.baseUrl);
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        url.searchParams.append(key, String(value));
      });
    }

    const response = await fetch(url.toString(), {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${this.token}`
      }
    });

    if (!response.ok) {
      throw new Error(`API Error: ${response.status}`);
    }

    return await response.json();
  }
}

// 사용 예시
const api = new ApiClient('http://localhost:5224', token);

// SellController
await api.get('/api/sell/events/schedules', { eventId: 1 });
await api.get('/api/sell/events/seat-options', { eventId: 1 });
await api.delete('/api/sell/tickets', { ticketId: 123 });

// ChatController
await api.get('/api/chat/rooms/detail', { roomId: 1 });
await api.get('/api/chat/messages', { roomId: 1, lastMessageId: 100, limit: 50 });
await api.post('/api/chat/rooms/read', { roomId: 1 });
await api.post('/api/chat/rooms/request-payment', { roomId: 1, transactionId: 456 });
await api.post('/api/chat/rooms/confirm-purchase', { roomId: 1, transactionId: 456 });
await api.post('/api/chat/rooms/cancel', {
  roomId: 1,
  transactionId: 456,
  cancelReason: '구매 의사 취소'
});
```

### Dart/Flutter

```dart
class ApiClient {
  final String baseUrl;
  final String token;

  ApiClient(this.baseUrl, this.token);

  Map<String, String> get headers => {
    'Authorization': 'Bearer $token',
    'Content-Type': 'application/json',
  };

  // GET with Query Parameters
  Future<Map<String, dynamic>> get(
    String endpoint,
    Map<String, dynamic>? queryParams
  ) async {
    final uri = Uri.parse('$baseUrl$endpoint')
      .replace(queryParameters: queryParams?.map(
        (key, value) => MapEntry(key, value.toString())
      ));

    final response = await http.get(uri, headers: headers);

    if (response.statusCode != 200) {
      throw Exception('API Error: ${response.statusCode}');
    }

    return jsonDecode(response.body);
  }

  // POST with JSON Body
  Future<Map<String, dynamic>> post(
    String endpoint,
    Map<String, dynamic> body
  ) async {
    final uri = Uri.parse('$baseUrl$endpoint');

    final response = await http.post(
      uri,
      headers: headers,
      body: jsonEncode(body),
    );

    if (response.statusCode != 200) {
      throw Exception('API Error: ${response.statusCode}');
    }

    return jsonDecode(response.body);
  }

  // DELETE with Query Parameters
  Future<Map<String, dynamic>> delete(
    String endpoint,
    Map<String, dynamic>? queryParams
  ) async {
    final uri = Uri.parse('$baseUrl$endpoint')
      .replace(queryParameters: queryParams?.map(
        (key, value) => MapEntry(key, value.toString())
      ));

    final response = await http.delete(uri, headers: headers);

    if (response.statusCode != 200) {
      throw Exception('API Error: ${response.statusCode}');
    }

    return jsonDecode(response.body);
  }
}

// 사용 예시
final api = ApiClient('http://localhost:5224', token);

// SellController
await api.get('/api/sell/events/schedules', {'eventId': 1});
await api.get('/api/sell/events/seat-options', {'eventId': 1});
await api.delete('/api/sell/tickets', {'ticketId': 123});

// ChatController
await api.get('/api/chat/rooms/detail', {'roomId': 1});
await api.get('/api/chat/messages', {'roomId': 1, 'lastMessageId': 100, 'limit': 50});
await api.post('/api/chat/rooms/read', {'roomId': 1});
await api.post('/api/chat/rooms/request-payment', {'roomId': 1, 'transactionId': 456});
await api.post('/api/chat/rooms/confirm-purchase', {'roomId': 1, 'transactionId': 456});
await api.post('/api/chat/rooms/cancel', {
  'roomId': 1,
  'transactionId': 456,
  'cancelReason': '구매 의사 취소'
});
```

---

## 🔗 관련 문서

- [API 리팩토링 요약](./api_refactoring_summary.md) - 리팩토링 배경 및 상세 내용
- [티켓 판매 API 스펙](./sell_ticket_api_spec.md) - SellController 전체 API 문서
- [채팅 API 스펙](./chat_api_spec.md) - ChatController 전체 API 문서

---

## 📞 문의

API 관련 문의사항이 있으시면 백엔드 팀에게 연락해주세요.

---

**마이그레이션 완료 후 이 체크리스트를 확인해주세요:**

- [ ] 모든 API 호출 코드를 새로운 방식으로 변경
- [ ] Query Parameter 및 Request Body 형식 확인
- [ ] 로컬 환경에서 테스트 완료
- [ ] 에러 처리 로직 확인
- [ ] TypeScript 타입 정의 업데이트 (해당되는 경우)

---

**작성일**: 2026-01-14
**작성자**: 백엔드 개발팀
**버전**: v2.0
