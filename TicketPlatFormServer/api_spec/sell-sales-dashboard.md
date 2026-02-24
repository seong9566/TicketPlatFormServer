# Sales Dashboard API

## Overview
The Sales Dashboard API provides endpoints for sellers to view their ticket sales performance grouped by events, with detailed ticket information and status tracking.

## Authentication
All endpoints require JWT Bearer token authentication.

## Endpoints

### GET /api/sell/sales-dashboard

**Description**: Event-grouped sales dashboard with status counts and pagination

**Authentication**: Required (JWT Bearer)

**Query Parameters**:
- `status` (string, optional): Filter by ticket status
  - `all` - All tickets (default)
  - `on_sale` - Currently on sale
  - `completed` - Sold out
  - `settling` - In settlement process
- `page` (integer, optional): Page number (default: 1, minimum: 1)
- `size` (integer, optional): Page size (default: 20, maximum: 100)

**Response**:
- Status Code: `200 OK`
- Content-Type: `application/json`

**Response Schema** (`SalesDashboardRespDto`):
```json
{
  "statusCode": 200,
  "message": "판매 대시보드 조회 성공",
  "data": {
    "eventGroups": [
      {
        "eventId": 1,
        "eventTitle": "BTS Concert 2024",
        "posterImageUrl": "https://example.com/poster.jpg",
        "venueName": "Seoul Olympic Gymnastics Arena",
        "earliestEventDatetime": "2024-06-15T19:00:00Z",
        "totalCount": 10,
        "onSaleCount": 5,
        "completedCount": 4,
        "settlingCount": 1
      },
      {
        "eventId": 2,
        "eventTitle": "BLACKPINK World Tour",
        "posterImageUrl": "https://example.com/poster2.jpg",
        "venueName": "Gocheok Sky Dome",
        "earliestEventDatetime": "2024-07-20T18:30:00Z",
        "totalCount": 8,
        "onSaleCount": 3,
        "completedCount": 5,
        "settlingCount": 0
      }
    ],
    "page": 1,
    "size": 20,
    "totalCount": 2,
    "hasMore": false
  }
}
```

**Status Filter Behavior**:
- **all**: Returns all events with all their tickets regardless of status
- **on_sale**: Returns only events that have at least one ticket in "on_sale" status
- **completed**: Returns only events that have at least one ticket in "completed" status
- **settling**: Returns only events that have at least one ticket in "settling" status

**Example Request**:
```bash
GET /api/sell/sales-dashboard?status=on_sale&page=1&size=20
Authorization: Bearer {jwt_token}
```

---

### GET /api/sell/sales-dashboard/{eventId}

**Description**: Individual ticket list for a specific event with detailed status information

**Authentication**: Required (JWT Bearer)

**Path Parameters**:
- `eventId` (integer, required): The ID of the event

**Query Parameters**:
- `page` (integer, optional): Page number (default: 1, minimum: 1)
- `size` (integer, optional): Page size (default: 20, maximum: 100)

**Response**:
- Status Code: `200 OK` or `404 Not Found`
- Content-Type: `application/json`

**Response Schema** (`EventTicketListRespDto`):
```json
{
  "statusCode": 200,
  "message": "공연별 판매 티켓 목록 조회 성공",
  "data": {
    "eventId": 1,
    "eventTitle": "BTS Concert 2024",
    "tickets": [
      {
        "ticketId": 101,
        "seatInfo": "VIP Section A, Row 5, Seat 10",
        "quantity": 2,
        "remainingQuantity": 1,
        "price": 150000,
        "originalPrice": 200000,
        "statusCode": "payment_completed",
        "statusName": "결제 완료",
        "transactionId": 5003,
        "thumbnailUrl": "https://example.com/ticket-101.jpg",
        "createdAt": "2024-05-10T14:30:00Z"
      },
      {
        "ticketId": 102,
        "seatInfo": "VIP Section B, Row 3, Seat 5",
        "quantity": 1,
        "remainingQuantity": 0,
        "price": 180000,
        "originalPrice": 200000,
        "statusCode": "payment_cancelled",
        "statusName": "결제 취소",
        "transactionId": 5001,
        "thumbnailUrl": "https://example.com/ticket-102.jpg",
        "createdAt": "2024-05-08T10:15:00Z"
      },
      {
        "ticketId": 103,
        "seatInfo": "General Admission",
        "quantity": 5,
        "remainingQuantity": 5,
        "price": 80000,
        "originalPrice": 100000,
        "statusCode": "settlement_completed",
        "statusName": "정산 완료",
        "transactionId": 5002,
        "thumbnailUrl": "https://example.com/ticket-103.jpg",
        "createdAt": "2024-05-05T09:00:00Z"
      }
    ],
    "page": 1,
    "size": 20,
    "totalCount": 3,
    "hasMore": false
  }
}
```

**Example Request**:
```bash
GET /api/sell/sales-dashboard/1?page=1&size=20
Authorization: Bearer {jwt_token}
```

---

## Status Mapping Rules

### Ticket Status Categories

| Status Code | Status Name | Description |
|------------|------------|-------------|
| `payment_completed` | 결제 완료 | Payment is completed and waiting for settlement |
| `payment_cancelled` | 결제 취소 | Payment was cancelled or refunded |
| `settlement_completed` | 정산 완료 | Settlement was completed |

### Status Transitions
1. **payment_completed** → **settlement_completed**: Settlement completes after payment confirmation
2. **payment_completed** → **payment_cancelled**: Payment is cancelled/refunded before settlement finalization

---

## Pagination

All list endpoints support pagination with the following parameters:

- **page**: Current page number (1-indexed, default: 1)
- **size**: Number of items per page (default: 20, max: 100)
- **totalCount**: Total number of items across all pages
- **hasMore**: Boolean indicating if more pages exist

**Example Pagination Flow**:
```
Request: GET /api/sell/sales-dashboard?page=1&size=20
Response: { page: 1, size: 20, totalCount: 45, hasMore: true }

Request: GET /api/sell/sales-dashboard?page=2&size=20
Response: { page: 2, size: 20, totalCount: 45, hasMore: true }

Request: GET /api/sell/sales-dashboard?page=3&size=20
Response: { page: 3, size: 20, totalCount: 45, hasMore: false }
```

---

## Error Responses

### 401 Unauthorized
```json
{
  "statusCode": 401,
  "message": "Unauthorized",
  "data": null
}
```
**Cause**: Missing or invalid JWT token

### 404 Not Found
```json
{
  "statusCode": 404,
  "message": "Event not found",
  "data": null
}
```
**Cause**: Event ID does not exist or user has no tickets for this event

### 400 Bad Request
```json
{
  "statusCode": 400,
  "message": "Invalid query parameters",
  "data": null
}
```
**Cause**: Invalid page, size, or status filter values

---

## Data Types

### SalesDashboardRespDto
| Field | Type | Description |
|-------|------|-------------|
| eventGroups | EventGroupItemDto[] | Array of event groups |
| page | int | Current page number |
| size | int | Page size |
| totalCount | int | Total number of events |
| hasMore | bool | Whether more pages exist |

### EventGroupItemDto
| Field | Type | Description |
|-------|------|-------------|
| eventId | int | Event identifier |
| eventTitle | string | Event name/title |
| posterImageUrl | string? | URL to event poster image |
| venueName | string? | Venue/location name |
| earliestEventDatetime | DateTime? | Earliest performance date |
| totalCount | int | Total tickets for this event |
| onSaleCount | int | Tickets currently on sale |
| completedCount | int | Sold out tickets |
| settlingCount | int | Tickets in settlement |

### EventTicketListRespDto
| Field | Type | Description |
|-------|------|-------------|
| eventId | int | Event identifier |
| eventTitle | string | Event name/title |
| tickets | EventTicketItemDto[] | Array of tickets |
| page | int | Current page number |
| size | int | Page size |
| totalCount | int | Total number of tickets |
| hasMore | bool | Whether more pages exist |

### EventTicketItemDto
| Field | Type | Description |
|-------|------|-------------|
| ticketId | int | Ticket identifier |
| seatInfo | string? | Seat location information |
| quantity | int | Original quantity listed |
| remainingQuantity | int | Quantity still available |
| price | int | Selling price (in KRW) |
| originalPrice | int | Original/face value price |
| statusCode | string | Status code (payment_completed/payment_cancelled/settlement_completed) |
| statusName | string | Human-readable status name |
| transactionId | long? | Associated transaction ID |
| thumbnailUrl | string? | Ticket image thumbnail URL |
| createdAt | DateTime | Ticket creation timestamp |

---

## Notes

- All prices are in Korean Won (KRW)
- Timestamps are in ISO 8601 format (UTC)
- Image URLs are valid for 7 days; use refresh endpoint to renew
- Event ticket detail list excludes `on_sale` items and shows payment/settlement histories only
- Status filter is case-insensitive
- Maximum page size is 100 items
