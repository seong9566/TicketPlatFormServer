# TASK-011 Payment Request Contract

## Endpoint

- `POST /api/payment/request`

## Request Body

| Field | Type | Required | Description |
|---|---|---|---|
| `transactionId` | long | yes | 결제 대상 거래 ID |
| `amount` | int | yes | 결제 금액 |
| `orderName` | string | yes | 토스 결제창 표시 주문명 |
| `customerName` | string | no | 구매자 이름 |
| `customerEmail` | string | no | 구매자 이메일 |

## Response Body

### Existing fields (backward compatible)

| Field | Type | Nullable | Description |
|---|---|---|---|
| `orderId` | string | no | 토스 결제용 주문 ID |
| `amount` | int | no | 결제 금액 |
| `orderName` | string | no | 토스 결제창 표시 주문명 |
| `customerName` | string | yes | 구매자 이름 |
| `customerEmail` | string | yes | 구매자 이메일 |
| `successUrl` | string | no | 결제 성공 리다이렉트 URL |
| `failUrl` | string | no | 결제 실패 리다이렉트 URL |
| `clientKey` | string | no | 토스 위젯 클라이언트 키 |

### Added fields (TASK-011)

| Field | Type | Nullable | Description |
|---|---|---|---|
| `ticketInfo` | object | yes | 결제 미리보기용 티켓 정보 |
| `eventInfo` | object | yes | 결제 미리보기용 공연 정보 |

#### `ticketInfo`

| Field | Type | Nullable | Description |
|---|---|---|---|
| `ticketId` | int | yes | 티켓 ID |
| `seatInfo` | string | yes | 좌석 요약 문자열 |
| `quantity` | int | yes | 총 구매 수량 |
| `unitPrice` | int | yes | 단가 |
| `totalAmount` | int | yes | 총 금액 |
| `thumbnailUrl` | string | yes | 이벤트 포스터/썸네일 |

#### `eventInfo`

| Field | Type | Nullable | Description |
|---|---|---|---|
| `eventId` | int | yes | 이벤트 ID |
| `title` | string | yes | 공연 제목 |
| `eventDateTime` | datetime | yes | 공연 일시 (UTC) |
| `venueName` | string | yes | 공연장 이름 |

## Nullability Rules

- 미리보기 조회가 실패하거나 일부 조인 데이터가 누락되어도 결제 요청 API는 정상 동작한다.
- 이 경우 `ticketInfo`/`eventInfo` 또는 하위 필드는 `null`일 수 있다.
- `orderId`, `amount`, `orderName`, `successUrl`, `failUrl`, `clientKey`는 항상 유지된다.

## Sample Response

```json
{
  "orderId": "TXN_123_5a4db90f30f14dfab1c039fbc7ce2674",
  "amount": 360000,
  "orderName": "Bunnies Camp 2024 티켓 결제",
  "customerName": "홍길동",
  "customerEmail": "buyer@test.com",
  "successUrl": "https://example.com/payment/success",
  "failUrl": "https://example.com/payment/fail",
  "clientKey": "test_ck_xxx",
  "ticketInfo": {
    "ticketId": 871,
    "seatInfo": "1층 R구역 VIP석 119열",
    "quantity": 1,
    "unitPrice": 360000,
    "totalAmount": 360000,
    "thumbnailUrl": "https://cdn.example.com/poster.png"
  },
  "eventInfo": {
    "eventId": 91,
    "title": "Bunnies Camp 2024",
    "eventDateTime": "2026-02-26T18:00:00Z",
    "venueName": "고척스카이돔"
  }
}
```
