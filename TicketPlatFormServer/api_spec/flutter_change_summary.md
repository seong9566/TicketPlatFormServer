# Flutter 전달용 API 변경 요약

**작성일**: 2026-02-03  
**대상**: Flutter 개발팀

---

## 변경 범위

- 채팅 기반 결제 요청: **수량 입력 + 즉시 재고 예약**
- 예약 만료 처리: **24시간 후 자동 취소 + 재고 복구 + 시스템 메시지**
- 구매 확정 메시지: `PURCHASE_CONFIRMED` 타입 추가
- 채팅방 나가기: **방 종료 처리 + 목록 제외**
- 결제 취소: **전액 취소 시 재고 복구**
- 채팅방 상세: **티켓 단가/총수량/남은수량 필드 추가**
- 채팅방 목록: **ticketThumbnailUrl(이벤트 포스터 이미지) 필드 추가**

---

## 1) Breaking Change: 결제 요청 API (수량 필수)

### `POST /api/chat/rooms/request-payment`

**변경 전**
```json
{
  "roomId": 1,
  "transactionId": 456
}
```

**변경 후**
```json
{
  "roomId": 1,
  "quantity": 2
}
```

**주요 동작 변경**
- `quantity`만큼 **즉시 재고 예약** (`remaining_quantity` 감소)
- `amount = ticket.price * quantity`
- `transactionId`는 요청 바디에서 제거됨

**에러 케이스**
- 수량 부족: `400 BadRequest` (`판매 가능한 수량이 부족합니다.`)
- 예약 만료: `400 BadRequest` (`결제 요청이 만료되었습니다.`)

---

## 2) 예약 만료 정책 (24시간)

**조건**: `pending_payment` 상태 + `ReservationExpiresAt < now`

**자동 처리**
- 거래 상태 `cancelled`
- 재고 복구
- 채팅방 시스템 메시지 추가
  - 메시지: `결제 요청이 만료되었습니다.`
  - 이벤트: `ReceiveMessage` (room_{roomId})

---

## 3) 구매 확정 메시지 타입 추가

### MessageType 추가
```
PURCHASE_CONFIRMED
```

**SignalR 이벤트**
```
Event: ReceiveMessage (room_{roomId})
{
  "messageId": 789,
  "roomId": 1,
  "senderId": 20,
  "senderNickname": "구매자",
  "message": null,
  "type": "PURCHASE_CONFIRMED",
  "createdAt": "2026-02-03T12:30:00Z"
}
```

**Note**: `message = null`이며, 타입으로 UI 카드 렌더링 필요

---

## 4) 채팅방 나가기 (Room Closed)

### `POST /api/chat/rooms/leave`

- 채팅방 상태 `closed`
- 목록에서 제외됨 (`ClosedAt` 존재 시 조회 제외)

**SignalR 이벤트**
```
Event: RoomUpdated
{
  "roomId": 1,
  "event": "RoomClosed",
  "statusCode": "closed",
  "message": "채팅방이 종료되었습니다."
}
```

---

## 5) 결제 취소 시 재고 복구 정책

- **전액 취소**: 재고 복구
- **부분 취소**: 재고 복구 안 함 (현재 미지원)

---

## 6) 채팅방 상세 응답 필드 추가

**대상**: `GET /api/chat/rooms/detail`

추가 필드:
```json
"ticket": {
  "unitPrice": 150000,
  "totalQuantity": 5,
  "remainingQuantity": 2
}
```

---

## 7) SignalR 그룹 규칙 (확인용)

- 채팅방: `room_{roomId}`
- 사용자: `user_{userId}`

---

## Flutter 변경 작업 체크리스트

1. 결제 요청 UI에 **수량 입력 Dialog** 추가
2. `request-payment` 호출 시 `quantity` 전달
3. 결제 금액 UI 업데이트: `price * quantity`
4. `PURCHASE_CONFIRMED` 메시지 타입 처리
5. 예약 만료 메시지 UI 처리 (텍스트 메시지)
6. 채팅방 종료 시 UI에서 목록 제거/닫기 처리

---

## 관련 문서

- `api_spec/chat_api_spec.md`
- `api_spec/PAYMENT_API_DOCS.md`
- `api_spec/purchase_confirmation_api.md`
