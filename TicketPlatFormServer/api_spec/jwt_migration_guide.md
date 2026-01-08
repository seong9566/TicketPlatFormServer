# JWT 인증 시스템 마이그레이션 가이드 (프론트엔드)

**배포 예정일**: TBD
**작성일**: 2026-01-08
**긴급도**: 🔴 높음 (기존 API 동작 변경)

---

## 📋 목차

1. [변경 사항 요약](#변경-사항-요약)
2. [마이그레이션 단계](#마이그레이션-단계)
3. [API 변경 상세](#api-변경-상세)
4. [새로운 API](#새로운-api)
5. [코드 예시](#코드-예시)
6. [테스트 체크리스트](#테스트-체크리스트)
7. [FAQ](#faq)

---

## 변경 사항 요약

### 🚨 Breaking Changes

1. **로그인 API 응답 변경**: Token 필드 추가
2. **Favorite API**: 인증 필수, userId 파라미터 제거
3. **Ticket Detail API**: userId 파라미터 제거

### ✅ 새로운 기능

1. JWT Token 발급 (Access Token + Refresh Token)
2. Token 자동 갱신 API
3. 로그아웃 API

### 🔐 보안 개선

- **Before**: userId를 파라미터로 전송 (타인 정보 접근 가능)
- **After**: JWT Token으로 사용자 인증 (안전함)

---

## 마이그레이션 단계

### Step 1: Token 저장 로직 구현 (우선순위: 최상)

로그인 성공 시 받은 Token을 안전하게 저장해야 합니다.

**권장 저장 위치**
- **Web**: `localStorage` 또는 `sessionStorage`
- **Mobile (Flutter)**: `flutter_secure_storage` 패키지

```typescript
// Web (TypeScript)
interface LoginResponse {
  id: number;
  email: string;
  role: string;
  provider: string;
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
  expiresAt: string;
}

async function login(email: string, password: string) {
  const response = await fetch('http://localhost:5000/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  });

  const data = await response.json();

  // Token 저장
  localStorage.setItem('accessToken', data.data.accessToken);
  localStorage.setItem('refreshToken', data.data.refreshToken);
  localStorage.setItem('expiresAt', data.data.expiresAt);

  return data;
}
```

```dart
// Flutter (Dart)
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class AuthService {
  final storage = FlutterSecureStorage();

  Future<void> login(String email, String password) async {
    final response = await http.post(
      Uri.parse('http://localhost:5000/auth/login'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'email': email, 'password': password}),
    );

    final data = jsonDecode(response.body);

    // Token 안전하게 저장
    await storage.write(key: 'accessToken', value: data['data']['accessToken']);
    await storage.write(key: 'refreshToken', value: data['data']['refreshToken']);
    await storage.write(key: 'expiresAt', value: data['data']['expiresAt']);
  }
}
```

### Step 2: HTTP Interceptor 구현 (우선순위: 최상)

모든 API 요청에 자동으로 Authorization 헤더를 추가합니다.

```typescript
// Web (Axios 예시)
import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5000'
});

// Request Interceptor: 모든 요청에 Token 추가
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response Interceptor: 401 에러 시 Token 갱신
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        // Token 갱신
        const refreshToken = localStorage.getItem('refreshToken');
        const response = await axios.post('http://localhost:5000/auth/refresh', {
          refreshToken
        });

        const { accessToken, refreshToken: newRefreshToken } = response.data.data;

        // 새 Token 저장
        localStorage.setItem('accessToken', accessToken);
        localStorage.setItem('refreshToken', newRefreshToken);

        // 원래 요청 재시도
        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        return api(originalRequest);
      } catch (refreshError) {
        // Refresh Token도 만료됨 → 로그아웃 처리
        localStorage.clear();
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default api;
```

```dart
// Flutter (Dio 예시)
import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class ApiService {
  final Dio dio = Dio();
  final storage = FlutterSecureStorage();

  ApiService() {
    dio.options.baseUrl = 'http://localhost:5000';

    // Request Interceptor
    dio.interceptors.add(InterceptorsWrapper(
      onRequest: (options, handler) async {
        final token = await storage.read(key: 'accessToken');
        if (token != null) {
          options.headers['Authorization'] = 'Bearer $token';
        }
        return handler.next(options);
      },
      onError: (error, handler) async {
        if (error.response?.statusCode == 401) {
          try {
            // Token 갱신
            final refreshToken = await storage.read(key: 'refreshToken');
            final response = await dio.post('/auth/refresh',
              data: {'refreshToken': refreshToken}
            );

            final newAccessToken = response.data['data']['accessToken'];
            final newRefreshToken = response.data['data']['refreshToken'];

            // 새 Token 저장
            await storage.write(key: 'accessToken', value: newAccessToken);
            await storage.write(key: 'refreshToken', value: newRefreshToken);

            // 원래 요청 재시도
            error.requestOptions.headers['Authorization'] = 'Bearer $newAccessToken';
            return handler.resolve(await dio.fetch(error.requestOptions));
          } catch (e) {
            // Refresh Token도 만료됨 → 로그아웃
            await storage.deleteAll();
            // Navigate to login
            return handler.reject(error);
          }
        }
        return handler.next(error);
      },
    ));
  }
}
```

### Step 3: API 호출 코드 수정

기존 코드에서 userId 파라미터를 제거합니다.

---

## API 변경 상세

### 1. POST /auth/login (로그인)

#### ⚠️ 변경 사항: 응답에 Token 필드 추가

**Request** (변경 없음)
```json
POST /auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response (Before)**
```json
{
  "message": "로그인 성공",
  "data": {
    "id": 1,
    "email": "user@example.com",
    "role": "user",
    "provider": "email",
    "lastLoginAt": "2026-01-08T10:00:00Z"
  },
  "statusCode": 200
}
```

**Response (After)** ⭐ 추가된 필드
```json
{
  "message": "로그인 성공",
  "data": {
    "id": 1,
    "email": "user@example.com",
    "role": "user",
    "provider": "email",
    "lastLoginAt": "2026-01-08T10:00:00Z",
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "550e8400-e29b-41d4-a716-446655440000",
    "expiresIn": 900,
    "tokenType": "Bearer",
    "expiresAt": "2026-01-08T10:15:00Z"
  },
  "statusCode": 200
}
```

#### 마이그레이션 작업
- ✅ 응답 타입에 Token 필드 추가
- ✅ Token을 localStorage/SecureStorage에 저장
- ✅ expiresAt 저장 (자동 갱신 로직에 사용)

---

### 2. POST /api/favorites/tickets (티켓 찜 토글)

#### 🔴 Breaking Change: 인증 필수, userId 파라미터 제거

**Request (Before)** ❌ 더 이상 작동하지 않음
```json
POST /api/favorites/tickets
Content-Type: application/json

{
  "userId": 123,  // ❌ 제거됨
  "ticketId": 456
}
```

**Request (After)** ✅ 새로운 방식
```json
POST /api/favorites/tickets
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "ticketId": 456  // userId는 서버에서 자동 추출
}
```

**Response** (변경 없음)
```json
{
  "message": "티켓 찜 추가 완료",
  "data": {
    "ticketId": 456,
    "isFavorited": true
  },
  "statusCode": 200
}
```

#### 에러 응답
```json
// 401 Unauthorized (Token 없음)
{
  "message": "인증 정보가 없습니다.",
  "data": null,
  "statusCode": 401
}
```

#### 마이그레이션 작업
- ❌ Request Body에서 `userId` 제거
- ✅ Authorization 헤더 추가
- ✅ 401 에러 처리 로직 추가

**코드 예시**
```typescript
// Before
async function toggleFavorite(userId: number, ticketId: number) {
  return await fetch('/api/favorites/tickets', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ userId, ticketId })  // ❌
  });
}

// After
async function toggleFavorite(ticketId: number) {
  const token = localStorage.getItem('accessToken');
  return await fetch('/api/favorites/tickets', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`  // ✅
    },
    body: JSON.stringify({ ticketId })  // userId 제거
  });
}
```

---

### 3. GET /api/favorites/tickets (찜한 티켓 목록 조회)

#### 🔴 Breaking Change: 인증 필수, userId 파라미터 제거

**Request (Before)** ❌ 더 이상 작동하지 않음
```http
GET /api/favorites/tickets?userId=123
```

**Request (After)** ✅ 새로운 방식
```http
GET /api/favorites/tickets
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response** (변경 없음)
```json
{
  "message": "찜한 티켓 목록 조회 성공",
  "data": [
    {
      "ticketId": 1,
      "ticketTitle": "아이유 콘서트 VIP석",
      "price": 150000,
      "favoritedAt": "2026-01-07T15:30:00Z"
    }
  ],
  "statusCode": 200
}
```

#### 마이그레이션 작업
- ❌ Query Parameter `userId` 제거
- ✅ Authorization 헤더 추가

**코드 예시**
```typescript
// Before
async function getFavorites(userId: number) {
  return await fetch(`/api/favorites/tickets?userId=${userId}`);  // ❌
}

// After
async function getFavorites() {
  const token = localStorage.getItem('accessToken');
  return await fetch('/api/favorites/tickets', {
    headers: { 'Authorization': `Bearer ${token}` }  // ✅
  });
}
```

---

### 4. GET /api/tickets/detail (티켓 상세 조회)

#### ℹ️ 선택적 변경: userId 파라미터 제거 (하위 호환 유지)

**Request (Before)** ⚠️ Deprecated (여전히 작동하지만 권장하지 않음)
```http
GET /api/tickets/detail?ticketId=123&userId=456
```

**Request (After)** ✅ 권장 방식
```http
# 로그인한 사용자
GET /api/tickets/detail?ticketId=123
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

# 로그인하지 않은 사용자 (찜 여부 확인 불가)
GET /api/tickets/detail?ticketId=123
```

**Response** (변경 없음)
```json
{
  "message": "티켓 상세 정보 조회 성공",
  "data": {
    "ticketId": 123,
    "ticketTitle": "아이유 콘서트 VIP석",
    "price": 150000,
    "isFavorited": true,  // 로그인한 경우만 값 있음, 아니면 null
    "seller": { ... }
  },
  "statusCode": 200
}
```

#### 마이그레이션 작업
- ❌ Query Parameter `userId` 제거 (선택적)
- ✅ Authorization 헤더 추가 (로그인한 경우)

**코드 예시**
```typescript
// Before
async function getTicketDetail(ticketId: number, userId?: number) {
  const url = userId
    ? `/api/tickets/detail?ticketId=${ticketId}&userId=${userId}`  // ❌
    : `/api/tickets/detail?ticketId=${ticketId}`;
  return await fetch(url);
}

// After
async function getTicketDetail(ticketId: number) {
  const token = localStorage.getItem('accessToken');
  const headers: HeadersInit = {};

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;  // ✅
  }

  return await fetch(`/api/tickets/detail?ticketId=${ticketId}`, { headers });
}
```

---

## 새로운 API

### 1. POST /auth/refresh (Token 갱신)

Access Token이 만료되었을 때 Refresh Token으로 새로운 Token을 발급받습니다.

**Request**
```json
POST /auth/refresh
Content-Type: application/json

{
  "refreshToken": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response (Success)**
```json
{
  "message": "Token 갱신 성공",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "660f9500-f30c-52e5-b827-557766551111",
    "expiresIn": 900,
    "tokenType": "Bearer",
    "expiresAt": "2026-01-08T11:30:00Z"
  },
  "statusCode": 200
}
```

**Response (Error)**
```json
// 401 Unauthorized (Refresh Token 만료/무효)
{
  "message": "만료되었거나 무효화된 Refresh Token입니다.",
  "data": null,
  "statusCode": 401
}
```

#### 사용 시점
- Access Token이 만료되었을 때 (401 에러)
- 앱 실행 시 Token 유효성 확인

**코드 예시**
```typescript
async function refreshToken() {
  const refreshToken = localStorage.getItem('refreshToken');

  const response = await fetch('/auth/refresh', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken })
  });

  if (!response.ok) {
    // Refresh Token도 만료 → 로그아웃
    localStorage.clear();
    window.location.href = '/login';
    throw new Error('Token refresh failed');
  }

  const data = await response.json();

  // 새 Token 저장
  localStorage.setItem('accessToken', data.data.accessToken);
  localStorage.setItem('refreshToken', data.data.refreshToken);
  localStorage.setItem('expiresAt', data.data.expiresAt);

  return data;
}
```

---

### 2. POST /auth/logout (로그아웃)

Refresh Token을 무효화하여 로그아웃합니다.

**Request**
```json
POST /auth/logout
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "refreshToken": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response (Success)**
```json
{
  "message": "로그아웃 성공",
  "data": null,
  "statusCode": 200
}
```

**코드 예시**
```typescript
async function logout() {
  const token = localStorage.getItem('accessToken');
  const refreshToken = localStorage.getItem('refreshToken');

  try {
    await fetch('/auth/logout', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ refreshToken })
    });
  } finally {
    // 성공 여부와 관계없이 로컬 Token 삭제
    localStorage.clear();
    window.location.href = '/login';
  }
}
```

---

## 코드 예시

### 완전한 Auth Service (TypeScript)

```typescript
// authService.ts
import axios from 'axios';

const API_BASE_URL = 'http://localhost:5000';

interface LoginRequest {
  email: string;
  password: string;
}

interface LoginResponse {
  id: number;
  email: string;
  role: string;
  provider: string;
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
  expiresAt: string;
}

class AuthService {
  private api = axios.create({ baseURL: API_BASE_URL });

  constructor() {
    // Request Interceptor
    this.api.interceptors.request.use(
      (config) => {
        const token = this.getAccessToken();
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      }
    );

    // Response Interceptor
    this.api.interceptors.response.use(
      (response) => response,
      async (error) => {
        const originalRequest = error.config;

        if (error.response?.status === 401 && !originalRequest._retry) {
          originalRequest._retry = true;

          try {
            await this.refreshToken();
            originalRequest.headers.Authorization = `Bearer ${this.getAccessToken()}`;
            return this.api(originalRequest);
          } catch (refreshError) {
            this.logout();
            throw refreshError;
          }
        }

        return Promise.reject(error);
      }
    );
  }

  async login(email: string, password: string): Promise<LoginResponse> {
    const response = await this.api.post('/auth/login', { email, password });
    const data = response.data.data;

    // Token 저장
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem('expiresAt', data.expiresAt);
    localStorage.setItem('user', JSON.stringify({
      id: data.id,
      email: data.email,
      role: data.role
    }));

    return data;
  }

  async refreshToken(): Promise<void> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    const response = await axios.post(`${API_BASE_URL}/auth/refresh`, {
      refreshToken
    });

    const data = response.data.data;
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem('expiresAt', data.expiresAt);
  }

  async logout(): Promise<void> {
    const refreshToken = this.getRefreshToken();

    try {
      if (refreshToken) {
        await this.api.post('/auth/logout', { refreshToken });
      }
    } finally {
      localStorage.clear();
      window.location.href = '/login';
    }
  }

  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  isAuthenticated(): boolean {
    const token = this.getAccessToken();
    const expiresAt = localStorage.getItem('expiresAt');

    if (!token || !expiresAt) {
      return false;
    }

    return new Date(expiresAt) > new Date();
  }

  getUser() {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }
}

export default new AuthService();
```

### API Service 사용 예시

```typescript
// favoriteService.ts
import authService from './authService';

class FavoriteService {
  async toggleFavorite(ticketId: number) {
    const response = await authService.api.post('/api/favorites/tickets', {
      ticketId  // userId는 서버에서 자동 추출
    });
    return response.data;
  }

  async getFavorites() {
    const response = await authService.api.get('/api/favorites/tickets');
    return response.data;
  }
}

export default new FavoriteService();
```

---

### 완전한 Auth Service (Flutter/Dart)

```dart
// auth_service.dart
import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class AuthService {
  final Dio _dio = Dio();
  final FlutterSecureStorage _storage = FlutterSecureStorage();
  static const String baseUrl = 'http://localhost:5000';

  AuthService() {
    _dio.options.baseUrl = baseUrl;
    _setupInterceptors();
  }

  void _setupInterceptors() {
    _dio.interceptors.add(InterceptorsWrapper(
      onRequest: (options, handler) async {
        final token = await _storage.read(key: 'accessToken');
        if (token != null) {
          options.headers['Authorization'] = 'Bearer $token';
        }
        return handler.next(options);
      },
      onError: (error, handler) async {
        if (error.response?.statusCode == 401) {
          try {
            await _refreshToken();

            // Retry original request
            final opts = error.requestOptions;
            final token = await _storage.read(key: 'accessToken');
            opts.headers['Authorization'] = 'Bearer $token';

            final response = await _dio.fetch(opts);
            return handler.resolve(response);
          } catch (e) {
            await logout();
            return handler.reject(error);
          }
        }
        return handler.next(error);
      },
    ));
  }

  Future<Map<String, dynamic>> login(String email, String password) async {
    final response = await _dio.post('/auth/login', data: {
      'email': email,
      'password': password,
    });

    final data = response.data['data'];

    // Token 저장
    await _storage.write(key: 'accessToken', value: data['accessToken']);
    await _storage.write(key: 'refreshToken', value: data['refreshToken']);
    await _storage.write(key: 'expiresAt', value: data['expiresAt']);
    await _storage.write(key: 'userId', value: data['id'].toString());
    await _storage.write(key: 'email', value: data['email']);
    await _storage.write(key: 'role', value: data['role']);

    return data;
  }

  Future<void> _refreshToken() async {
    final refreshToken = await _storage.read(key: 'refreshToken');
    if (refreshToken == null) {
      throw Exception('No refresh token available');
    }

    final response = await _dio.post('/auth/refresh', data: {
      'refreshToken': refreshToken,
    });

    final data = response.data['data'];
    await _storage.write(key: 'accessToken', value: data['accessToken']);
    await _storage.write(key: 'refreshToken', value: data['refreshToken']);
    await _storage.write(key: 'expiresAt', value: data['expiresAt']);
  }

  Future<void> logout() async {
    final refreshToken = await _storage.read(key: 'refreshToken');

    try {
      if (refreshToken != null) {
        await _dio.post('/auth/logout', data: {
          'refreshToken': refreshToken,
        });
      }
    } finally {
      await _storage.deleteAll();
      // Navigate to login screen
    }
  }

  Future<bool> isAuthenticated() async {
    final token = await _storage.read(key: 'accessToken');
    final expiresAt = await _storage.read(key: 'expiresAt');

    if (token == null || expiresAt == null) {
      return false;
    }

    final expiryDate = DateTime.parse(expiresAt);
    return expiryDate.isAfter(DateTime.now());
  }

  Dio get dio => _dio;
}

// favorite_service.dart
class FavoriteService {
  final AuthService _authService;

  FavoriteService(this._authService);

  Future<Map<String, dynamic>> toggleFavorite(int ticketId) async {
    final response = await _authService.dio.post('/api/favorites/tickets',
      data: {'ticketId': ticketId}
    );
    return response.data;
  }

  Future<List<dynamic>> getFavorites() async {
    final response = await _authService.dio.get('/api/favorites/tickets');
    return response.data['data'];
  }
}
```

---

## 테스트 체크리스트

### 로그인 플로우
- [ ] 로그인 성공 시 Token이 올바르게 저장되는가?
- [ ] 로그인 성공 후 보호된 API 접근이 가능한가?
- [ ] 잘못된 비밀번호로 로그인 시 적절한 에러 메시지가 표시되는가?

### Token 관리
- [ ] 모든 인증이 필요한 API에 Authorization 헤더가 자동으로 추가되는가?
- [ ] Access Token 만료 시 자동으로 Refresh Token으로 갱신되는가?
- [ ] Refresh Token도 만료된 경우 로그인 페이지로 리다이렉트되는가?

### Favorite API
- [ ] 티켓 찜 추가/해제가 정상적으로 동작하는가?
- [ ] userId를 파라미터로 전송하지 않아도 동작하는가?
- [ ] 로그인하지 않은 상태에서 접근 시 401 에러가 발생하는가?
- [ ] 찜한 티켓 목록 조회가 정상적으로 동작하는가?

### Ticket Detail API
- [ ] 로그인한 상태에서 티켓 상세 조회 시 isFavorited 값이 올바른가?
- [ ] 로그인하지 않은 상태에서도 티켓 상세 조회가 가능한가?
- [ ] userId 파라미터 없이도 정상 동작하는가?

### 로그아웃
- [ ] 로그아웃 후 Token이 로컬에서 삭제되는가?
- [ ] 로그아웃 후 보호된 API 접근 시 401 에러가 발생하는가?

### 에러 처리
- [ ] 네트워크 에러 시 적절한 에러 메시지가 표시되는가?
- [ ] 401 에러 시 자동으로 Token 갱신을 시도하는가?
- [ ] 403 에러 시 권한 없음 메시지가 표시되는가?

---

## FAQ

### Q1. 기존에 저장된 userId는 어떻게 처리하나요?

**A**: 기존 코드에서 localStorage에 저장했던 userId는 더 이상 필요하지 않습니다. 로그인 시 받은 user 정보에 id가 포함되어 있으므로, 필요한 경우 그것을 사용하면 됩니다. 하지만 API 요청 시에는 userId를 전송하지 않아도 됩니다 (서버에서 JWT Token으로 자동 추출).

### Q2. Token은 어디에 저장해야 하나요?

**A**:
- **Web**: localStorage 권장 (sessionStorage도 가능하지만 탭 닫으면 삭제됨)
- **Mobile**: flutter_secure_storage 필수 (일반 SharedPreferences는 보안상 비권장)

### Q3. Access Token이 만료되면 어떻게 되나요?

**A**: 서버에서 401 에러를 반환합니다. 이때 자동으로 Refresh Token으로 새로운 Access Token을 발급받아야 합니다. 위의 Interceptor 코드 예시를 참고하세요.

### Q4. Refresh Token도 만료되면 어떻게 되나요?

**A**: 사용자를 로그인 페이지로 리다이렉트해야 합니다. Refresh Token은 7일 동안 유효하므로, 7일 이상 앱을 사용하지 않으면 다시 로그인해야 합니다.

### Q5. 기존 API 호출 코드를 모두 수정해야 하나요?

**A**: Interceptor를 구현하면 대부분의 코드는 수정할 필요가 없습니다. 다만:
- userId를 파라미터로 전송하던 부분은 제거해야 합니다.
- 로그인 응답 처리 코드는 Token 저장 로직을 추가해야 합니다.

### Q6. 개발 중에 Token 없이 테스트하고 싶어요.

**A**: 서버 측에서 특정 엔드포인트에 대해 일시적으로 [AllowAnonymous] 속성을 추가할 수 있습니다. 하지만 프로덕션에는 절대 배포하지 마세요!

### Q7. Token이 계속 만료되어 불편해요.

**A**:
- Access Token 만료 시간: 15분 (서버 설정)
- Refresh Token 만료 시간: 7일 (서버 설정)

개발 중에는 서버의 appsettings.json에서 `AccessTokenExpirationMinutes`를 늘릴 수 있습니다. 하지만 보안을 위해 프로덕션에서는 15분 이하를 권장합니다.

### Q8. 여러 탭/창에서 동시에 사용할 때 Token이 동기화되나요?

**A**: localStorage는 같은 도메인의 모든 탭/창에서 공유됩니다. 하지만 한 탭에서 Token을 갱신했을 때 다른 탭에서 즉시 반영되지 않을 수 있습니다. 이 경우 `storage` 이벤트를 감지하여 동기화할 수 있습니다:

```typescript
window.addEventListener('storage', (e) => {
  if (e.key === 'accessToken') {
    // Token이 변경됨 → 페이지 새로고침 또는 상태 업데이트
    window.location.reload();
  }
});
```

### Q9. Token을 쿠키에 저장하는 것과 비교하면?

**A**:
- **localStorage**: 프론트엔드에서 완전히 제어 가능, 간단함
- **HttpOnly Cookie**: XSS 공격에 안전하지만 CSRF 공격에 취약, CORS 설정 필요

현재 구현은 localStorage 방식이므로 XSS 공격에 주의해야 합니다. 외부 스크립트를 로드하지 마세요!

---

## 배포 전 확인사항

### 프론트엔드

- [ ] Token 저장 로직 구현 완료
- [ ] HTTP Interceptor 구현 완료
- [ ] 모든 Favorite API 호출 코드에서 userId 파라미터 제거
- [ ] Ticket Detail API 호출 코드에서 userId 파라미터 제거
- [ ] 로그아웃 기능 구현 완료
- [ ] 401 에러 처리 로직 구현 완료
- [ ] 테스트 체크리스트 완료

### 백엔드

- [ ] JWT Secret Key를 환경 변수로 관리 (appsettings.json에서 제거)
- [ ] HTTPS 강제 설정
- [ ] CORS 설정 확인
- [ ] Refresh Token 정리 작업 스케줄러 구현 (선택)

---

## 지원

문제가 발생하거나 질문이 있으면 백엔드 팀에 문의하세요.

**작성자**: 백엔드 개발팀
**최종 수정일**: 2026-01-08
**버전**: 1.0.0
