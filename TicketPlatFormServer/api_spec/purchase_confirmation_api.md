# 구매확정 API 명세서

**버전**: 1.0  
**최종 업데이트**: 2026-02-02  
**대상**: Flutter 개발자

---

## 개요

구매확정(Purchase Confirmation) API는 구매자가 티켓을 정상적으로 받았음을 확인하는 엔드포인트입니다. 구매확정 시 다음 작업이 자동으로 수행됩니다:

1. ✅ **Escrow 해제** - 판매자에게 정산 시작
2. ✅ **Settlement 생성** - 정산 레코드 자동 생성 (D+1 스케줄링)
3. ✅ **티켓 소유권 이전** - `remaining_quantity` 감소
4. ✅ **PURCHASE_CONFIRMED 메시지 생성** - 채팅방에 자동 메시지
5. ✅ **실시간 알림** - SignalR로 구매자/판매자에게 알림
6. ✅ **채팅방 잠금** - 더 이상 메시지 송수신 불가

---

## 1. 구매 확정 API

### 엔드포인트
```
POST /api/chat/rooms/confirm-purchase
```

### 인증
- **필수**: JWT Bearer Token

### 요청 헤더
```http
Authorization: Bearer {access_token}
Content-Type: application/json
```

### 요청 본문

```json
{
  "roomId": 1,
  "transactionId": 123
}
```

#### 요청 필드 설명

| 필드 | 타입 | 필수 | 설명 |
|------|------|------|------|
| `roomId` | `long` | ✅ | 채팅방 ID |
| `transactionId` | `long` | ✅ | 거래 ID |

> **참고**: `userId`는 JWT 토큰에서 자동으로 추출됩니다.

---

### 성공 응답 (200 OK)

```json
{
  "statusCode": 200,
  "message": "구매 확정이 완료되었습니다",
  "data": {
    "transactionId": 123,
    "confirmedAt": "2026-02-02T10:30:00Z",
    "success": true
  },
  "timestamp": "2026-02-02T10:30:00.123Z"
}
```

#### 응답 필드 설명

| 필드 | 타입 | 설명 |
|------|------|------|
| `data.transactionId` | `long` | 확정된 거래 ID |
| `data.confirmedAt` | `string (ISO 8601)` | 구매 확정 시각 (UTC) |
| `data.success` | `boolean` | 성공 여부 (항상 `true`) |

---

### 오류 응답

#### 1. 인증 실패 (401 Unauthorized)
```json
{
  "statusCode": 401,
  "message": "인증 정보가 없습니다.",
  "data": null,
  "timestamp": "2026-02-02T10:30:00.123Z"
}
```

#### 2. 권한 없음 (403 Forbidden)
```json
{
  "statusCode": 403,
  "message": "이 채팅방에 접근할 권한이 없습니다.",
  "data": null,
  "timestamp": "2026-02-02T10:30:00.123Z"
}
```

**발생 조건**:
- 채팅방 멤버가 아닌 경우
- 구매자가 아닌 사용자가 구매확정 시도 (판매자는 불가)

#### 3. 채팅방 없음 (404 Not Found)
```json
{
  "statusCode": 404,
  "message": "채팅방을 찾을 수 없습니다.",
  "data": null,
  "timestamp": "2026-02-02T10:30:00.123Z"
}
```

#### 4. 에스크로 없음 (404 Not Found)
```json
{
  "statusCode": 404,
  "message": "에스크로를 찾을 수 없습니다.",
  "data": null,
  "timestamp": "2026-02-02T10:30:00.123Z"
}
```

**발생 조건**: 결제가 완료되지 않은 거래

#### 5. 거래 정보 불일치 (400 Bad Request)
```json
{
  "statusCode": 400,
  "message": "거래 정보가 일치하지 않습니다.",
  "data": null,
  "timestamp": "2026-02-02T10:30:00.123Z"
}
```

**발생 조건**: `transactionId`가 채팅방의 거래와 일치하지 않음

#### 6. 서버 오류 (500 Internal Server Error)
```json
{
  "statusCode": 500,
  "message": "에스크로 해제 중 오류가 발생했습니다.",
  "data": null,
  "timestamp": "2026-02-02T10:30:00.123Z"
}
```

---

## 2. SignalR 실시간 알림

구매확정 성공 시 다음 SignalR 이벤트가 **자동으로 발송**됩니다:

### 2.1. 채팅방 멤버에게 메시지 전송

**이벤트**: `ReceiveMessage`  
**채널**: `ChatRoom_{roomId}`

```json
{
  "messageId": 456,
  "roomId": 1,
  "senderId": 10,
  "senderNickname": "구매자닉네임",
  "message": null,
  "type": "PURCHASE_CONFIRMED",
  "createdAt": "2026-02-02T10:30:00Z"
}
```

### 2.2. 구매자/판매자 개인 알림

**이벤트**: `NewMessage`  
**채널**: `User_{userId}` (구매자, 판매자 각각)

```json
{
  "messageId": 456,
  "roomId": 1,
  "senderId": 10,
  "senderNickname": "구매자닉네임",
  "message": null,
  "type": "PURCHASE_CONFIRMED",
  "createdAt": "2026-02-02T10:30:00Z"
}
```

> **중요**: `type: "PURCHASE_CONFIRMED"` 메시지는 `message` 필드가 `null`입니다. Flutter에서 이 타입을 확인하여 **구매확정 완료 카드 UI**를 렌더링하세요.

---

## 3. 메시지 타입 (MessageType)

### Enum 정의

```typescript
enum MessageType {
  TEXT = "TEXT",                        // 일반 텍스트
  IMAGE = "IMAGE",                      // 이미지
  PAYMENT_REQUEST = "PAYMENT_REQUEST",  // 결제 요청
  PAYMENT_SUCCESS = "PAYMENT_SUCCESS",  // 결제 완료
  PURCHASE_CONFIRMED = "PURCHASE_CONFIRMED"  // 구매 확정 ⭐
}
```

### Flutter에서 렌더링 예시

```dart
Widget buildChatMessage(ChatMessage message) {
  switch (message.type) {
    case 'TEXT':
      return Text(message.message);
    
    case 'IMAGE':
      return Image.network(message.imageUrl);
    
    case 'PAYMENT_REQUEST':
      return PaymentRequestCard(message);
    
    case 'PAYMENT_SUCCESS':
      return PaymentSuccessCard(message);
    
    case 'PURCHASE_CONFIRMED':
      return PurchaseConfirmedCard(message);  // ⭐ 새로 추가
    
    default:
      return Text(message.message ?? '');
  }
}
```

#### PurchaseConfirmedCard 예시

```dart
class PurchaseConfirmedCard extends StatelessWidget {
  final ChatMessage message;
  
  @override
  Widget build(BuildContext context) {
    return Card(
      color: Colors.green[50],
      child: Padding(
        padding: EdgeInsets.all(16),
        child: Column(
          children: [
            Icon(Icons.check_circle, color: Colors.green, size: 40),
            SizedBox(height: 8),
            Text(
              '구매가 확정되었습니다',
              style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
            ),
            SizedBox(height: 4),
            Text(
              '판매자에게 정산이 진행됩니다',
              style: TextStyle(fontSize: 14, color: Colors.grey[600]),
            ),
            SizedBox(height: 4),
            Text(
              DateFormat('yyyy-MM-dd HH:mm').format(message.createdAt),
              style: TextStyle(fontSize: 12, color: Colors.grey),
            ),
          ],
        ),
      ),
    );
  }
}
```

---

## 4. 전체 플로우 (End-to-End)

```
┌─────────────┐
│ 1. 판매 등록 │
└──────┬──────┘
       │ POST /api/sell/transactions
       │ → Transaction 생성
       │ → ChatRoom 생성
       v
┌─────────────┐
│ 2. 결제 요청 │
└──────┬──────┘
       │ (채팅방에서 판매자가 요청)
       │ → PAYMENT_REQUEST 메시지 생성
       v
┌─────────────┐
│ 3. 결제 완료 │
└──────┬──────┘
       │ POST /api/payment/confirm
       │ → Payment, Escrow 생성
       │ → PAYMENT_SUCCESS 메시지 자동 생성
       v
┌─────────────┐
│ 4. 구매 확정 │ ⭐ 이 API
└──────┬──────┘
       │ POST /api/chat/rooms/confirm-purchase
       │
       ├─ Escrow 해제 (status: released)
       ├─ Transaction 상태 변경 (status: confirmed)
       ├─ Settlement 생성 (D+1 정산 스케줄링)
       ├─ Ticket.RemainingQuantity 감소
       ├─ PURCHASE_CONFIRMED 메시지 생성
       ├─ SignalR 실시간 알림 전송
       └─ ChatRoom 잠금
       v
┌─────────────┐
│  거래 완료   │
└─────────────┘
```

---

## 5. 구매확정 버튼 표시 조건

채팅방 상세 정보 API (`GET /api/chat/rooms/{roomId}`)의 응답에서 확인:

```json
{
  "statusCode": 200,
  "data": {
    "roomId": 1,
    "transaction": {
      "id": 123,
      "statusCode": "paid",
      "confirmedAt": null
    },
    "canConfirmPurchase": true,  // ⭐ 이 필드 확인
    // ...
  }
}
```

### canConfirmPurchase 조건

```
canConfirmPurchase = true  // 다음 조건을 모두 만족할 때
  ├─ 현재 사용자가 구매자인가?
  ├─ transaction이 존재하는가?
  └─ transaction.confirmedAt == null인가? (아직 확정 안 됨)
```

**Flutter 예시**:
```dart
if (chatRoom.canConfirmPurchase) {
  ElevatedButton(
    onPressed: () => confirmPurchase(),
    child: Text('구매 확정'),
  );
}
```

---

## 6. 중복 구매확정 방지

### 멱등성 보장

동일한 거래에 대해 구매확정을 여러 번 호출해도 안전합니다:

1. **첫 번째 호출**: 정상 처리 (200 OK)
2. **두 번째 호출**: Escrow가 이미 해제됨 (ReleasedAt != null) → 로그만 남기고 성공 반환

### Flutter에서 처리

```dart
bool _isConfirming = false;

Future<void> confirmPurchase() async {
  if (_isConfirming) return;  // 중복 호출 방지
  
  setState(() => _isConfirming = true);
  
  try {
    await apiService.confirmPurchase(roomId, transactionId);
    // 성공 처리
  } catch (e) {
    // 에러 처리
  } finally {
    setState(() => _isConfirming = false);
  }
}
```

---

## 7. 정산(Settlement) 정보

구매확정 후 자동으로 생성되는 정산 정보:

### Settlement 데이터 구조

```json
{
  "id": 789,
  "transactionId": 123,
  "sellerId": 20,
  "amount": 150000,        // 총 금액
  "fee": 5250,             // 수수료 (3.5%)
  "netAmount": 144750,     // 순 정산 금액 (판매자 수령액)
  "bankAccountId": 5,      // 판매자 계좌 ID
  "statusId": 1,           // pending (정산 대기)
  "scheduledAt": "2026-02-03T10:30:00Z",  // D+1 정산 예정일
  "processedAt": null,
  "createdAt": "2026-02-02T10:30:00Z"
}
```

### 정산 상태 (SettlementStatus)

| 코드 | 이름 | 설명 |
|------|------|------|
| `pending` | 정산 대기 | 구매확정 완료, 정산 대기중 |
| `processing` | 정산 처리중 | 정산 진행중 |
| `completed` | 정산 완료 | 판매자에게 입금 완료 |
| `failed` | 정산 실패 | 정산 처리 실패 (재시도 필요) |

---

## 8. 티켓 수량 감소

구매확정 시 자동으로 티켓의 `remaining_quantity`가 감소합니다:

### 예시

**구매 전**:
```json
{
  "ticketId": 50,
  "quantity": 10,
  "remainingQuantity": 7  // 3장 예약중
}
```

**구매자가 2장 구매확정 후**:
```json
{
  "ticketId": 50,
  "quantity": 10,
  "remainingQuantity": 5  // 7 - 2 = 5
}
```

> **참고**: `TransactionItems`를 순회하며 모든 티켓의 수량을 감소시킵니다. 여러 티켓 동시 구매 지원.

---

## 9. 에러 처리 가이드

### Flutter에서 권장 에러 처리

```dart
Future<void> confirmPurchase(int roomId, int transactionId) async {
  try {
    final response = await apiService.post(
      '/api/chat/rooms/confirm-purchase',
      body: {
        'roomId': roomId,
        'transactionId': transactionId,
      },
    );
    
    if (response.statusCode == 200) {
      // 성공: UI 업데이트 (구매확정 버튼 비활성화 등)
      showSuccessDialog('구매가 확정되었습니다');
    }
    
  } on UnauthorizedException {
    // 401: 로그인 화면으로 이동
    navigateToLogin();
    
  } on ForbiddenException catch (e) {
    // 403: 권한 없음
    if (e.message.contains('구매자만')) {
      showErrorDialog('구매자만 구매확정할 수 있습니다');
    } else {
      showErrorDialog('채팅방 접근 권한이 없습니다');
    }
    
  } on NotFoundException catch (e) {
    // 404: 리소스 없음
    if (e.message.contains('채팅방')) {
      showErrorDialog('채팅방을 찾을 수 없습니다');
    } else if (e.message.contains('에스크로')) {
      showErrorDialog('결제가 완료되지 않았습니다');
    }
    
  } on BadRequestException {
    // 400: 잘못된 요청
    showErrorDialog('거래 정보가 올바르지 않습니다');
    
  } on ServerException {
    // 500: 서버 오류
    showErrorDialog('서버 오류가 발생했습니다. 잠시 후 다시 시도해주세요');
  }
}
```

---

## 10. 테스트 시나리오

### 정상 플로우 테스트

```dart
test('구매확정 성공', () async {
  // Given: 결제 완료된 거래
  final roomId = 1;
  final transactionId = 123;
  
  // When: 구매확정 API 호출
  final response = await confirmPurchase(roomId, transactionId);
  
  // Then: 성공 응답
  expect(response.statusCode, 200);
  expect(response.data.success, true);
  expect(response.data.transactionId, transactionId);
});
```

### 에러 케이스 테스트

```dart
test('판매자가 구매확정 시도 시 403 에러', () async {
  // Given: 판매자 JWT 토큰
  apiService.setToken(sellerToken);
  
  // When: 구매확정 API 호출
  final response = confirmPurchase(roomId, transactionId);
  
  // Then: 403 Forbidden
  expect(response, throwsA(isA<ForbiddenException>()));
});
```

---

## 11. FAQ

### Q1. 구매확정 후 채팅방에서 메시지를 보낼 수 있나요?
**A**: 아니요. 구매확정 시 채팅방이 자동으로 잠금됩니다. 더 이상 메시지를 송수신할 수 없습니다.

### Q2. PURCHASE_CONFIRMED 메시지는 수동으로 생성할 수 있나요?
**A**: 아니요. `PURCHASE_CONFIRMED` 메시지는 **오직 서버에서만** 자동으로 생성됩니다. 클라이언트에서 임의로 생성하면 안 됩니다.

### Q3. Settlement은 언제 생성되나요?
**A**: 구매확정 시 자동으로 생성됩니다. 결제 완료 시점에는 생성되지 않습니다.

### Q4. 정산은 언제 이루어지나요?
**A**: 구매확정 다음 날(D+1)로 스케줄링됩니다. `scheduledAt` 필드를 확인하세요.

### Q5. 판매자 계좌가 없으면 어떻게 되나요?
**A**: Settlement 레코드는 생성되지만 `bankAccountId = 0`으로 설정됩니다. 정산 처리 시 판매자에게 계좌 등록을 요청하게 됩니다.

### Q6. 구매확정 후 취소할 수 있나요?
**A**: 아니요. 구매확정은 **되돌릴 수 없는 작업**입니다. 구매확정 버튼을 누르기 전에 확인 다이얼로그를 표시하는 것을 권장합니다.

```dart
void showConfirmDialog() {
  showDialog(
    context: context,
    builder: (context) => AlertDialog(
      title: Text('구매 확정'),
      content: Text('구매를 확정하시겠습니까?\n확정 후에는 취소할 수 없습니다.'),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context),
          child: Text('취소'),
        ),
        ElevatedButton(
          onPressed: () {
            Navigator.pop(context);
            confirmPurchase();
          },
          child: Text('확정'),
        ),
      ],
    ),
  );
}
```

---

## 12. 관련 API 문서

- **채팅방 조회**: `chat_api_spec.md`
- **결제 API**: `PAYMENT_API_DOCS.md`
- **SignalR 실시간 알림**: `signalr_realtime_notification_api.md`

---

**문서 버전**: 1.0  
**최종 업데이트**: 2026-02-02  
**작성자**: Backend Team
