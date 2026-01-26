# 티켓 결제 API 문서 (Flutter 개발자용)

> 토스페이먼츠 기반 중고 티켓 결제 시스템 API

**Version**: 1.0
**Last Updated**: 2026-01-26
**Base URL**: `http://localhost:5000/api` (개발), `https://api.tickethub.com/api` (운영)

---

## 목차
1. [인증](#인증)
2. [결제 플로우 개요](#결제-플로우-개요)
3. [API 엔드포인트](#api-엔드포인트)
4. [데이터 모델](#데이터-모델)
5. [에러 코드](#에러-코드)
6. [Flutter 예제 코드](#flutter-예제-코드)

---

## 인증

모든 API 요청은 JWT Bearer 토큰이 필요합니다 (Webhook 제외).

```dart
// HTTP 헤더에 추가
headers: {
  'Authorization': 'Bearer YOUR_JWT_TOKEN',
  'Content-Type': 'application/json',
}
```

---

## 결제 플로우 개요

### 기본 플로우 (채팅 기반)

```
[1] 판매자: 채팅방에서 "결제 요청" 버튼 클릭
    → POST /api/chat/rooms/request-payment
    → Transaction 생성 (pending_payment) + TransactionItem 생성
    → OrderId 생성, paymentUrl/transactionId/amount 반환
    ↓
[2] Flutter: paymentUrl로 결제 화면 오픈 (WebView/외부 브라우저)
    ↓
[3] 토스: 결제 성공 시 SuccessUrl로 리다이렉트
    - paymentKey, orderId, amount 파라미터 포함
    ↓
[4] Flutter: POST /api/payment/confirm
    - 백엔드에서 토스 API 승인 호출
    - Payment + Escrow 생성
    - Transaction 상태: paid
    ↓
[5] 구매자: "구매 확정" 버튼 클릭
    - POST /api/chat/rooms/confirm-purchase
    - 에스크로 해제
    - Transaction 상태: confirmed
    ↓
[6] 정산 완료 처리 (백오피스/배치)
    - Transaction 상태: completed
```

**참고**
- 채팅 기반 플로우에서는 `/api/chat/rooms/request-payment`를 사용합니다. 결제 위젯을 직접 붙이는 경우에만 `/api/payment/request`를 사용합니다.
- 결제 요청 전에는 채팅방 상세의 `transaction`이 `null`입니다.

### 트랜잭션 상태

`transaction_statuses` 테이블 기준입니다.

| ID | Code | Name (KR) | 설명 |
|----|------|-----------|------|
| 1 | reserved | 예약중 | 결제 요청 전 예약 단계 |
| 2 | pending_payment | 결제대기 | 결제 요청 후 결제 대기 |
| 3 | paid | 결제완료 | 결제 승인 완료 |
| 4 | confirmed | 구매확정 | 구매 확정(에스크로 해제) |
| 5 | completed | 거래완료 | 정산 완료 |
| 6 | cancelled | 취소됨 | 거래 취소 |
| 7 | refunded | 환불됨 | 환불 완료 |

### 상태별 화면 가이드 (Flutter)

| Status Code | 상태명 | 구매자 화면/액션 | 판매자 화면/액션 |
|-------------|--------|------------------|------------------|
| reserved | 예약중 | 채팅 가능, 결제 요청 대기 | 결제 요청 버튼 활성 |
| pending_payment | 결제대기 | "결제하기" CTA, 취소 가능 | "결제 요청됨" 안내, 취소 가능 |
| paid | 결제완료 | "구매 확정" CTA | "결제 완료" 안내, 확정 대기 |
| confirmed | 구매확정 | 확정 완료, 거래 완료 대기 | 확정 완료, 정산 진행 안내 |
| completed | 거래완료 | 거래 완료, 후기/평가 노출 | 거래 완료, 후기/평가 노출 |
| cancelled | 취소됨 | 취소 사유 표시, 재요청 가능 | 취소 사유 표시 |
| refunded | 환불됨 | 환불 완료, 환불 금액/일시 표시 | 환불 완료 안내 |

---

## API 엔드포인트

### 1. 결제 요청 준비

채팅방에서 결제 요청 시 호출하여 OrderId를 생성합니다.

#### **POST** `/api/payment/request`

**Headers**:
```
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json
```

**Request Body**:
```json
{
  "transactionId": 123,
  "amount": 50000,
  "orderName": "아이유 콘서트 티켓",
  "customerName": "홍길동",
  "customerEmail": "hong@example.com"
}
```

**Response** (200 OK):
```json
{
  "message": "결제 요청 준비 완료",
  "data": {
    "orderId": "TXN_123_a7f3b2c14d5e6f7g8h9i0j1k2l3m4n5o",
    "amount": 50000,
    "orderName": "아이유 콘서트 티켓",
    "customerName": "홍길동",
    "customerEmail": "hong@example.com",
    "clientKey": "test_ck_O6BYq7GWPVvglqzGdNwrNE5vbo1d",
    "successUrl": "http://localhost:5173/payment/success",
    "failUrl": "http://localhost:5173/payment/fail"
  },
  "statusCode": 200
}
```

**Flutter 예제**:
```dart
Future<PaymentRequestResponse> requestPayment({
  required int transactionId,
  required int amount,
  required String orderName,
}) async {
  final response = await http.post(
    Uri.parse('$baseUrl/payment/request'),
    headers: {
      'Authorization': 'Bearer $token',
      'Content-Type': 'application/json',
    },
    body: jsonEncode({
      'transactionId': transactionId,
      'amount': amount,
      'orderName': orderName,
      'customerName': userProfile.name,
      'customerEmail': userProfile.email,
    }),
  );

  if (response.statusCode == 200) {
    final json = jsonDecode(response.body);
    return PaymentRequestResponse.fromJson(json['data']);
  } else {
    throw Exception('결제 요청 실패');
  }
}
```

---

### 2. 결제 승인

토스페이먼츠 결제 위젯에서 결제 완료 후 호출합니다.

#### **POST** `/api/payment/confirm`

**Headers**:
```
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json
```

**Request Body**:
```json
{
  "paymentKey": "5zJ4xY7m0kODnyRpQWGrN2xqGlNvLrKwv1M9ENjbeoPaZdL6",
  "orderId": "TXN_123_a7f3b2c14d5e6f7g8h9i0j1k2l3m4n5o",
  "amount": 50000
}
```

**Response** (200 OK):
```json
{
  "message": "결제 승인 완료",
  "data": {
    "paymentKey": "5zJ4xY7m0kODnyRpQWGrN2xqGlNvLrKwv1M9ENjbeoPaZdL6",
    "orderId": "TXN_123_a7f3b2c14d5e6f7g8h9i0j1k2l3m4n5o",
    "orderName": "아이유 콘서트 티켓",
    "status": "DONE",
    "requestedAt": "2026-01-26T10:30:00+09:00",
    "approvedAt": "2026-01-26T10:30:15+09:00",
    "method": "카드",
    "totalAmount": 50000,
    "balanceAmount": 50000,
    "card": {
      "company": "신한",
      "number": "1234****5678",
      "installmentPlanMonths": 0,
      "cardType": "신용",
      "ownerType": "개인"
    }
  },
  "statusCode": 200
}
```

**Flutter 예제**:
```dart
Future<TossPaymentResponse> confirmPayment({
  required String paymentKey,
  required String orderId,
  required int amount,
}) async {
  final response = await http.post(
    Uri.parse('$baseUrl/payment/confirm'),
    headers: {
      'Authorization': 'Bearer $token',
      'Content-Type': 'application/json',
    },
    body: jsonEncode({
      'paymentKey': paymentKey,
      'orderId': orderId,
      'amount': amount,
    }),
  );

  if (response.statusCode == 200) {
    final json = jsonDecode(response.body);
    return TossPaymentResponse.fromJson(json['data']);
  } else {
    throw Exception('결제 승인 실패');
  }
}
```

---

### 3. 결제 조회

OrderId로 결제 정보를 조회합니다.

#### **GET** `/api/payment/order/{orderId}`

**Headers**:
```
Authorization: Bearer {JWT_TOKEN}
```

**Response** (200 OK):
```json
{
  "message": "결제 조회 완료",
  "data": {
    "paymentKey": "5zJ4xY7m0kODnyRpQWGrN2xqGlNvLrKwv1M9ENjbeoPaZdL6",
    "orderId": "TXN_123_a7f3b2c14d5e6f7g8h9i0j1k2l3m4n5o",
    "status": "DONE",
    "totalAmount": 50000,
    "method": "카드"
  },
  "statusCode": 200
}
```

**Flutter 예제**:
```dart
Future<TossPaymentResponse> getPayment(String orderId) async {
  final response = await http.get(
    Uri.parse('$baseUrl/payment/order/$orderId'),
    headers: {
      'Authorization': 'Bearer $token',
    },
  );

  if (response.statusCode == 200) {
    final json = jsonDecode(response.body);
    return TossPaymentResponse.fromJson(json['data']);
  } else {
    throw Exception('결제 조회 실패');
  }
}
```

---

### 4. 결제 취소 (환불)

결제를 취소하고 환불 처리합니다.

#### **POST** `/api/payment/cancel`

**Headers**:
```
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json
```

**Request Body**:
```json
{
  "transactionId": 123,
  "cancelReason": "단순 변심",
  "cancelAmount": null  // null이면 전액 취소
}
```

**Response** (200 OK):
```json
{
  "message": "결제 취소 완료",
  "data": {
    "paymentKey": "5zJ4xY7m0kODnyRpQWGrN2xqGlNvLrKwv1M9ENjbeoPaZdL6",
    "orderId": "TXN_123_a7f3b2c14d5e6f7g8h9i0j1k2l3m4n5o",
    "status": "CANCELLED",
    "cancelAmount": 50000,
    "cancelReason": "단순 변심",
    "canceledAt": "2026-01-26T11:00:00Z"
  },
  "statusCode": 200
}
```

**Flutter 예제**:
```dart
Future<PaymentCancelResponse> cancelPayment({
  required int transactionId,
  required String cancelReason,
  int? cancelAmount,
}) async {
  final response = await http.post(
    Uri.parse('$baseUrl/payment/cancel'),
    headers: {
      'Authorization': 'Bearer $token',
      'Content-Type': 'application/json',
    },
    body: jsonEncode({
      'transactionId': transactionId,
      'cancelReason': cancelReason,
      'cancelAmount': cancelAmount,
    }),
  );

  if (response.statusCode == 200) {
    final json = jsonDecode(response.body);
    return PaymentCancelResponse.fromJson(json['data']);
  } else {
    throw Exception('결제 취소 실패');
  }
}
```

---

## 데이터 모델

### PaymentRequestResponse

```dart
class PaymentRequestResponse {
  final String orderId;
  final int amount;
  final String orderName;
  final String? customerName;
  final String? customerEmail;
  final String clientKey;
  final String successUrl;
  final String failUrl;

  PaymentRequestResponse({
    required this.orderId,
    required this.amount,
    required this.orderName,
    this.customerName,
    this.customerEmail,
    required this.clientKey,
    required this.successUrl,
    required this.failUrl,
  });

  factory PaymentRequestResponse.fromJson(Map<String, dynamic> json) {
    return PaymentRequestResponse(
      orderId: json['orderId'],
      amount: json['amount'],
      orderName: json['orderName'],
      customerName: json['customerName'],
      customerEmail: json['customerEmail'],
      clientKey: json['clientKey'],
      successUrl: json['successUrl'],
      failUrl: json['failUrl'],
    );
  }
}
```

### TossPaymentResponse

```dart
class TossPaymentResponse {
  final String paymentKey;
  final String orderId;
  final String orderName;
  final String status;
  final String requestedAt;
  final String? approvedAt;
  final String method;
  final int totalAmount;
  final int balanceAmount;
  final TossCard? card;
  final TossVirtualAccount? virtualAccount;

  TossPaymentResponse({
    required this.paymentKey,
    required this.orderId,
    required this.orderName,
    required this.status,
    required this.requestedAt,
    this.approvedAt,
    required this.method,
    required this.totalAmount,
    required this.balanceAmount,
    this.card,
    this.virtualAccount,
  });

  factory TossPaymentResponse.fromJson(Map<String, dynamic> json) {
    return TossPaymentResponse(
      paymentKey: json['paymentKey'],
      orderId: json['orderId'],
      orderName: json['orderName'],
      status: json['status'],
      requestedAt: json['requestedAt'],
      approvedAt: json['approvedAt'],
      method: json['method'],
      totalAmount: json['totalAmount'],
      balanceAmount: json['balanceAmount'],
      card: json['card'] != null ? TossCard.fromJson(json['card']) : null,
      virtualAccount: json['virtualAccount'] != null
          ? TossVirtualAccount.fromJson(json['virtualAccount'])
          : null,
    );
  }
}

class TossCard {
  final String company;
  final String number;
  final int installmentPlanMonths;
  final String cardType;
  final String ownerType;

  TossCard({
    required this.company,
    required this.number,
    required this.installmentPlanMonths,
    required this.cardType,
    required this.ownerType,
  });

  factory TossCard.fromJson(Map<String, dynamic> json) {
    return TossCard(
      company: json['company'],
      number: json['number'],
      installmentPlanMonths: json['installmentPlanMonths'],
      cardType: json['cardType'],
      ownerType: json['ownerType'],
    );
  }
}

class TossVirtualAccount {
  final String accountNumber;
  final String bankCode;
  final String customerName;
  final String dueDate;
  final bool expired;

  TossVirtualAccount({
    required this.accountNumber,
    required this.bankCode,
    required this.customerName,
    required this.dueDate,
    required this.expired,
  });

  factory TossVirtualAccount.fromJson(Map<String, dynamic> json) {
    return TossVirtualAccount(
      accountNumber: json['accountNumber'],
      bankCode: json['bankCode'],
      customerName: json['customerName'],
      dueDate: json['dueDate'],
      expired: json['expired'],
    );
  }
}
```

### PaymentCancelResponse

```dart
class PaymentCancelResponse {
  final String paymentKey;
  final String orderId;
  final String status;
  final int cancelAmount;
  final String cancelReason;
  final DateTime canceledAt;

  PaymentCancelResponse({
    required this.paymentKey,
    required this.orderId,
    required this.status,
    required this.cancelAmount,
    required this.cancelReason,
    required this.canceledAt,
  });

  factory PaymentCancelResponse.fromJson(Map<String, dynamic> json) {
    return PaymentCancelResponse(
      paymentKey: json['paymentKey'],
      orderId: json['orderId'],
      status: json['status'],
      cancelAmount: json['cancelAmount'],
      cancelReason: json['cancelReason'],
      canceledAt: DateTime.parse(json['canceledAt']),
    );
  }
}
```

---

## 에러 코드

### HTTP 상태 코드

| 코드 | 설명 | 예시 |
|-----|------|------|
| 200 | 성공 | 요청이 정상적으로 처리됨 |
| 400 | Bad Request | 잘못된 요청 파라미터, 금액 불일치 |
| 401 | Unauthorized | JWT 토큰 누락 또는 만료 |
| 403 | Forbidden | 권한 없음 (타인의 결제 취소 시도 등) |
| 404 | Not Found | OrderId 또는 TransactionId 없음 |
| 409 | Conflict | 중복 결제 시도 (이미 처리된 결제) |
| 500 | Internal Server Error | 서버 내부 오류 |

### 에러 응답 형식

```json
{
  "message": "거래를 찾을 수 없습니다.",
  "statusCode": 404
}
```

### 주요 에러 메시지

| 메시지 | 원인 | 해결 방법 |
|--------|------|----------|
| "인증 정보가 없습니다." | JWT 토큰 누락 | Authorization 헤더 확인 |
| "거래를 찾을 수 없습니다." | 잘못된 transactionId | transactionId 확인 |
| "결제 금액이 일치하지 않습니다." | 금액 불일치 | 요청 amount와 실제 결제 금액 확인 |
| "유효하지 않은 OrderId 형식입니다." | OrderId 형식 오류 | TXN_숫자_GUID 형식 확인 |
| "결제 취소 권한이 없습니다." | 타인의 결제 취소 시도 | 로그인 사용자 확인 |
| "결제 정보를 찾을 수 없습니다." | 잘못된 orderId | orderId 확인 |

---

## Flutter 예제 코드

### 1. 토스페이먼츠 위젯 통합

#### pubspec.yaml
```yaml
dependencies:
  flutter:
    sdk: flutter
  http: ^1.1.0
  webview_flutter: ^4.4.2  # 토스 결제 위젯용
```

#### 결제 요청 및 위젯 열기

```dart
import 'package:flutter/material.dart';
import 'package:webview_flutter/webview_flutter.dart';
import 'dart:convert';
import 'package:http/http.dart' as http;

class PaymentScreen extends StatefulWidget {
  final int transactionId;
  final int amount;
  final String orderName;

  const PaymentScreen({
    required this.transactionId,
    required this.amount,
    required this.orderName,
  });

  @override
  _PaymentScreenState createState() => _PaymentScreenState();
}

class _PaymentScreenState extends State<PaymentScreen> {
  late WebViewController _webViewController;
  bool _isLoading = true;
  String? _orderId;

  @override
  void initState() {
    super.initState();
    _initPayment();
  }

  Future<void> _initPayment() async {
    try {
      // Step 1: 결제 요청 (OrderId 생성)
      final response = await http.post(
        Uri.parse('$baseUrl/payment/request'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode({
          'transactionId': widget.transactionId,
          'amount': widget.amount,
          'orderName': widget.orderName,
          'customerName': currentUser.name,
          'customerEmail': currentUser.email,
        }),
      );

      if (response.statusCode == 200) {
        final json = jsonDecode(response.body);
        final data = json['data'];

        setState(() {
          _orderId = data['orderId'];
        });

        // Step 2: 토스 결제 위젯 HTML 생성
        final html = _generateTossPaymentHTML(
          clientKey: data['clientKey'],
          orderId: data['orderId'],
          amount: data['amount'],
          orderName: data['orderName'],
          customerName: data['customerName'],
          successUrl: data['successUrl'],
          failUrl: data['failUrl'],
        );

        // Step 3: WebView에 로드
        _webViewController = WebViewController()
          ..setJavaScriptMode(JavaScriptMode.unrestricted)
          ..setNavigationDelegate(
            NavigationDelegate(
              onPageStarted: (String url) {
                if (url.contains('payment/success')) {
                  _handlePaymentSuccess(url);
                } else if (url.contains('payment/fail')) {
                  _handlePaymentFail(url);
                }
              },
            ),
          )
          ..loadHtmlString(html);

        setState(() {
          _isLoading = false;
        });
      } else {
        throw Exception('결제 요청 실패');
      }
    } catch (e) {
      _showError('결제 요청 실패: $e');
    }
  }

  String _generateTossPaymentHTML({
    required String clientKey,
    required String orderId,
    required int amount,
    required String orderName,
    String? customerName,
    required String successUrl,
    required String failUrl,
  }) {
    return '''
    <!DOCTYPE html>
    <html>
    <head>
      <meta charset="utf-8">
      <meta name="viewport" content="width=device-width, initial-scale=1">
      <script src="https://js.tosspayments.com/v1"></script>
    </head>
    <body>
      <div id="payment-method"></div>
      <button id="payment-button" style="
        width: 100%;
        padding: 16px;
        background-color: #3182f6;
        color: white;
        border: none;
        border-radius: 8px;
        font-size: 16px;
        font-weight: bold;
        cursor: pointer;
        margin-top: 20px;
      ">결제하기</button>

      <script>
        const tossPayments = TossPayments('$clientKey');
        const paymentWidget = tossPayments.widgets({ customerKey: 'CUSTOMER_KEY_${DateTime.now().millisecondsSinceEpoch}' });

        paymentWidget.renderPaymentMethods('#payment-method', { value: $amount });

        document.getElementById('payment-button').addEventListener('click', function() {
          paymentWidget.requestPayment({
            orderId: '$orderId',
            orderName: '$orderName',
            customerName: '$customerName',
            successUrl: '$successUrl',
            failUrl: '$failUrl',
          });
        });
      </script>
    </body>
    </html>
    ''';
  }

  Future<void> _handlePaymentSuccess(String url) async {
    try {
      // URL에서 파라미터 추출
      final uri = Uri.parse(url);
      final paymentKey = uri.queryParameters['paymentKey']!;
      final orderId = uri.queryParameters['orderId']!;
      final amount = int.parse(uri.queryParameters['amount']!);

      // Step 4: 결제 승인 요청
      final response = await http.post(
        Uri.parse('$baseUrl/payment/confirm'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode({
          'paymentKey': paymentKey,
          'orderId': orderId,
          'amount': amount,
        }),
      );

      if (response.statusCode == 200) {
        Navigator.of(context).pop();
        _showSuccess('결제가 완료되었습니다!');
      } else {
        throw Exception('결제 승인 실패');
      }
    } catch (e) {
      _showError('결제 승인 실패: $e');
    }
  }

  void _handlePaymentFail(String url) {
    final uri = Uri.parse(url);
    final message = uri.queryParameters['message'] ?? '결제 실패';
    _showError(message);
  }

  void _showSuccess(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message), backgroundColor: Colors.green),
    );
  }

  void _showError(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message), backgroundColor: Colors.red),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('결제')),
      body: _isLoading
          ? Center(child: CircularProgressIndicator())
          : WebViewWidget(controller: _webViewController),
    );
  }
}
```

### 2. 결제 취소 (환불)

```dart
Future<void> cancelPayment(BuildContext context, int transactionId) async {
  final confirmed = await showDialog<bool>(
    context: context,
    builder: (context) => AlertDialog(
      title: Text('결제 취소'),
      content: Text('정말로 결제를 취소하시겠습니까?'),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context, false),
          child: Text('아니오'),
        ),
        TextButton(
          onPressed: () => Navigator.pop(context, true),
          child: Text('예'),
        ),
      ],
    ),
  );

  if (confirmed == true) {
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/payment/cancel'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode({
          'transactionId': transactionId,
          'cancelReason': '단순 변심',
        }),
      );

      if (response.statusCode == 200) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('결제가 취소되었습니다.'), backgroundColor: Colors.green),
        );
      } else {
        throw Exception('결제 취소 실패');
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('결제 취소 실패: $e'), backgroundColor: Colors.red),
      );
    }
  }
}
```

### 3. 결제 상태 확인

```dart
class PaymentStatusScreen extends StatefulWidget {
  final String orderId;

  const PaymentStatusScreen({required this.orderId});

  @override
  _PaymentStatusScreenState createState() => _PaymentStatusScreenState();
}

class _PaymentStatusScreenState extends State<PaymentStatusScreen> {
  TossPaymentResponse? _payment;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadPayment();
  }

  Future<void> _loadPayment() async {
    try {
      final response = await http.get(
        Uri.parse('$baseUrl/payment/order/${widget.orderId}'),
        headers: {
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final json = jsonDecode(response.body);
        setState(() {
          _payment = TossPaymentResponse.fromJson(json['data']);
          _isLoading = false;
        });
      } else {
        throw Exception('결제 조회 실패');
      }
    } catch (e) {
      setState(() {
        _isLoading = false;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('결제 조회 실패: $e')),
      );
    }
  }

  String _getStatusText(String status) {
    switch (status) {
      case 'DONE':
        return '결제 완료';
      case 'CANCELLED':
        return '결제 취소';
      case 'WAITING_FOR_DEPOSIT':
        return '입금 대기';
      default:
        return status;
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return Scaffold(
        appBar: AppBar(title: Text('결제 정보')),
        body: Center(child: CircularProgressIndicator()),
      );
    }

    if (_payment == null) {
      return Scaffold(
        appBar: AppBar(title: Text('결제 정보')),
        body: Center(child: Text('결제 정보를 찾을 수 없습니다.')),
      );
    }

    return Scaffold(
      appBar: AppBar(title: Text('결제 정보')),
      body: ListView(
        padding: EdgeInsets.all(16),
        children: [
          _buildInfoRow('주문번호', _payment!.orderId),
          _buildInfoRow('상품명', _payment!.orderName),
          _buildInfoRow('결제 상태', _getStatusText(_payment!.status)),
          _buildInfoRow('결제 수단', _payment!.method),
          _buildInfoRow('결제 금액', '${_payment!.totalAmount.toString()}원'),
          if (_payment!.card != null) ...[
            Divider(),
            Text('카드 정보', style: TextStyle(fontWeight: FontWeight.bold)),
            _buildInfoRow('카드사', _payment!.card!.company),
            _buildInfoRow('카드 번호', _payment!.card!.number),
            _buildInfoRow('할부', '${_payment!.card!.installmentPlanMonths}개월'),
          ],
        ],
      ),
    );
  }

  Widget _buildInfoRow(String label, String value) {
    return Padding(
      padding: EdgeInsets.symmetric(vertical: 8),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(label, style: TextStyle(color: Colors.grey[600])),
          Text(value, style: TextStyle(fontWeight: FontWeight.bold)),
        ],
      ),
    );
  }
}
```

---

## 테스트 가이드

### 테스트 카드 정보

토스페이먼츠 개발 환경에서 사용할 수 있는 테스트 카드:

| 카드번호 | 결과 | 비고 |
|---------|------|------|
| 5123-4567-8901-2346 | 성공 | 일반 신용카드 |
| 4123-4567-8901-2345 | 성공 | 체크카드 |
| 카드번호 끝자리 0000 | 실패 | 잔액 부족 |
| 카드번호 끝자리 0001 | 실패 | 도난/분실 카드 |

**유효기간**: 임의 (미래 날짜)
**CVC**: 임의 3자리
**비밀번호**: 임의 2자리

### 테스트 시나리오

1. **정상 결제 플로우**
   - 채팅방 → 결제 요청 → 결제 위젯 → 테스트 카드 입력 → 결제 승인 → 완료

2. **결제 실패**
   - 잘못된 카드 정보 입력
   - 금액 불일치
   - 네트워크 오류

3. **결제 취소**
   - 결제 완료 후 즉시 취소
   - 결제 내역 확인

---

## FAQ

### Q1. 토스페이먼츠 위젯이 표시되지 않아요
**A**: WebView JavaScript 모드가 활성화되어 있는지 확인하세요.
```dart
_webViewController.setJavaScriptMode(JavaScriptMode.unrestricted);
```

### Q2. 결제 승인 후 중복으로 호출되나요?
**A**: 아니요. 백엔드에서 OrderId 기반 Idempotency 체크를 하므로, 중복 요청 시 기존 결제 정보를 반환합니다.

### Q3. 환불은 얼마나 걸리나요?
**A**: 카드사별로 3~7영업일 소요됩니다.

### Q4. 가상계좌 결제는 어떻게 처리하나요?
**A**: 가상계좌 선택 시 `TossVirtualAccount` 객체에 계좌번호와 입금 기한이 포함됩니다. 사용자에게 표시 후 입금 대기 상태가 됩니다.

### Q5. Webhook은 어떻게 처리하나요?
**A**: Webhook은 백엔드에서 자동 처리되며, Flutter 앱에서는 별도 처리가 필요 없습니다.

---

## 지원

**문의**: dev@tickethub.com
**개발자 문서**: https://docs.tosspayments.com
**API 버전**: 1.0

---

**Last Updated**: 2026-01-26
