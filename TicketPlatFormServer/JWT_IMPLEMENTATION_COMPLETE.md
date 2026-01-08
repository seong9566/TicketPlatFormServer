# JWT 인증 시스템 구현 완료 보고서

**작성일**: 2026-01-08
**프로젝트**: TicketPlatForm Server
**구현 기간**: Phase 1~3 완료

---

## 📋 목차

1. [개요](#개요)
2. [구현 배경](#구현-배경)
3. [Phase 1: JWT 인프라 구축](#phase-1-jwt-인프라-구축)
4. [Phase 2: 로그인 API 수정](#phase-2-로그인-api-수정)
5. [Phase 3: 기존 API 마이그레이션](#phase-3-기존-api-마이그레이션)
6. [생성/수정된 파일 목록](#생성수정된-파일-목록)
7. [데이터베이스 변경사항](#데이터베이스-변경사항)
8. [보안 개선사항](#보안-개선사항)
9. [테스트 가이드](#테스트-가이드)
10. [향후 개선 사항](#향후-개선-사항)

---

## 개요

ASP.NET Core 9.0 기반 티켓 플랫폼 서버에 JWT(JSON Web Token) 인증 시스템을 구현하여 보안을 강화하고 사용자 인증 체계를 확립했습니다.

### 주요 성과

- ✅ JWT 기반 인증/인가 시스템 구축
- ✅ Access Token (15분) + Refresh Token (7일) 이중 토큰 전략
- ✅ Token Rotation 보안 패턴 구현
- ✅ 기존 API의 보안 취약점 해결
- ✅ Claims 기반 사용자 정보 추출

### 기술 스택

- **Framework**: ASP.NET Core 9.0
- **Authentication**: JWT Bearer Authentication
- **Package**: Microsoft.AspNetCore.Authentication.JwtBearer 9.0.0
- **Algorithm**: HS256 (HMAC SHA-256)
- **Database**: MySQL 9.4.0

---

## 구현 배경

### 기존 시스템의 문제점

1. **보안 취약점**: userId를 쿼리 파라미터/바디로 직접 전달
   ```bash
   # 타인의 정보 접근 가능 (심각한 보안 취약점)
   GET /api/favorites/tickets?userId=123
   POST /api/favorites/tickets { "userId": 123, "ticketId": 456 }
   ```

2. **인증 시스템 부재**: 모든 API가 공개 상태
3. **세션 관리 불가능**: 로그아웃, 토큰 무효화 기능 없음
4. **사용자 검증 불가**: 요청자가 본인인지 확인할 수 없음

### 해결 방안

JWT 기반 인증 시스템을 도입하여:
- 사용자 신원 검증
- API별 접근 제어
- 안전한 토큰 기반 세션 관리
- Claims를 통한 사용자 정보 추출

---

## Phase 1: JWT 인프라 구축

### 1.1 NuGet 패키지 추가

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
<PackageReference Include="Microsoft.IdentityModel.Tokens" Version="8.2.1" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.2.1" />
```

### 1.2 JWT 설정 (`appsettings.json`)

```json
{
  "JwtSettings": {
    "SecretKey": "TicketPlatform-SuperSecret-JWT-Key-Min-256-Bits-For-HS256-Algorithm-2026",
    "Issuer": "TicketPlatform",
    "Audience": "TicketPlatformClient",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7,
    "ValidateIssuerSigningKey": true,
    "ValidateIssuer": true,
    "ValidateAudience": true,
    "ValidateLifetime": true,
    "ClockSkew": 0
  }
}
```

### 1.3 생성된 핵심 파일

#### Config/JwtSettings.cs
JWT 설정을 담는 클래스

#### Services/Token/TokenService.cs
- `GenerateTokensAsync()`: Access Token + Refresh Token 생성
- `GenerateAccessToken()`: JWT 생성 (userId, email, role, provider Claims 포함)
- `GenerateRefreshToken()`: UUID 형식 Refresh Token 생성
- `ValidateToken()`: Token 유효성 검증
- `GetUserIdFromToken()`: Token에서 UserId 추출

#### Repository/Token/RefreshTokenRepository.cs
- `SaveRefreshTokenAsync()`: Refresh Token DB 저장
- `GetRefreshTokenAsync()`: Token 조회
- `RevokeRefreshTokenAsync()`: Token 무효화 (Token Rotation)
- `RevokeAllUserTokensAsync()`: 사용자의 모든 Token 무효화
- `RemoveExpiredTokensAsync()`: 만료된 Token 삭제
- `IsTokenValidAsync()`: Token 유효성 확인

#### Common/ClaimsExtensions.cs
Claims에서 사용자 정보 추출하는 확장 메서드

```csharp
public static class ClaimsExtensions
{
    public static int? GetUserId(this ClaimsPrincipal user);
    public static string? GetEmail(this ClaimsPrincipal user);
    public static string? GetRole(this ClaimsPrincipal user);
    public static string? GetProvider(this ClaimsPrincipal user);
}
```

### 1.4 Program.cs 미들웨어 등록

```csharp
// JWT 설정 읽기
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);
builder.Services.AddSingleton(jwtSettings);

// JWT 인증 미들웨어
var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(jwtSettings.ClockSkew)
        };
    });

// 서비스 등록
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// 미들웨어 파이프라인
app.UseAuthentication();
app.UseAuthorization();
```

---

## Phase 2: 로그인 API 수정

### 2.1 LoginUserRespDto에 Token 필드 추가

```csharp
public class LoginUserRespDto
{
    // 기존 필드
    public int Id { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string Provider { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // 추가된 Token 필드
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public int? ExpiresIn { get; set; }
    public string? TokenType { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
```

### 2.2 UserService.LoginUser 수정

```csharp
public async Task<LoginUserRespDto> LoginUser(LoginUserReqDto dto)
{
    // 1~3. 기존 검증 로직 (이메일, 비밀번호)

    // 4. JWT Token 생성
    var tokenResponse = await _tokenService.GenerateTokensAsync(user, 7);

    // 5. Refresh Token DB 저장
    var refreshToken = new DBModel.RefreshToken
    {
        UserId = user.Id,
        Token = tokenResponse.RefreshToken,
        ExpiryDate = DateTime.UtcNow.AddDays(7)
    };
    await _refreshTokenRepo.SaveRefreshTokenAsync(refreshToken);

    // 6. 응답에 Token 정보 포함
    return new LoginUserRespDto
    {
        // ... 기존 필드
        AccessToken = tokenResponse.AccessToken,
        RefreshToken = tokenResponse.RefreshToken,
        ExpiresIn = tokenResponse.ExpiresIn,
        TokenType = tokenResponse.TokenType,
        ExpiresAt = tokenResponse.ExpiresAt
    };
}
```

### 2.3 AuthController 신규 엔드포인트

#### POST /auth/refresh - Access Token 갱신

```csharp
[HttpPost("refresh")]
public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenReqDto dto)
{
    // 1. Refresh Token 조회 및 검증
    var refreshToken = await _refreshTokenRepo.GetRefreshTokenAsync(dto.RefreshToken);
    var isValid = await _refreshTokenRepo.IsTokenValidAsync(dto.RefreshToken);

    // 2. 새로운 Token 생성
    var newTokenResponse = await _tokenService.GenerateTokensAsync(refreshToken.User, 7);

    // 3. 이전 Token 무효화 (Token Rotation)
    await _refreshTokenRepo.RevokeRefreshTokenAsync(dto.RefreshToken, newTokenResponse.RefreshToken);

    // 4. 새 Refresh Token 저장
    await _refreshTokenRepo.SaveRefreshTokenAsync(newRefreshToken);

    return Ok(newTokenResponse);
}
```

#### POST /auth/logout - 로그아웃

```csharp
[Authorize]
[HttpPost("logout")]
public async Task<IActionResult> Logout([FromBody] RefreshTokenReqDto dto)
{
    // Refresh Token 무효화
    await _refreshTokenRepo.RevokeRefreshTokenAsync(dto.RefreshToken);
    return Ok();
}
```

---

## Phase 3: 기존 API 마이그레이션

### 3.1 FavoriteController 보안 강화

**Before (보안 취약)**
```csharp
[HttpPost("tickets")]
public async Task<IActionResult> ToggleTicketFavorite([FromBody] FavoriteToggleReqDto req)
{
    // userId를 클라이언트가 전송 (타인 userId 전송 가능)
    var result = await _favoriteService.ToggleTicketFavorite(req);
    return Ok(result);
}
```

**After (보안 강화)**
```csharp
[Authorize]  // 인증 필수
[HttpPost("tickets")]
public async Task<IActionResult> ToggleTicketFavorite([FromBody] FavoriteToggleReqDto req)
{
    // Claims에서 userId 추출 (위조 불가능)
    var userId = User.GetUserId();
    if (userId == null)
        throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

    req.UserId = userId.Value;
    var result = await _favoriteService.ToggleTicketFavorite(req);
    return Ok(result);
}
```

### 3.2 FavoriteToggleReqDto 수정

```csharp
public class FavoriteToggleReqDto
{
    // [JsonIgnore] 추가: 클라이언트는 userId 전송하지 않음
    [JsonIgnore]
    public int UserId { get; set; }

    public int TicketId { get; set; }
}
```

### 3.3 TicketController 수정

**Before**
```csharp
[HttpGet("detail")]
public async Task<IActionResult> GetTicketDetail([FromQuery] int ticketId, [FromQuery] int? userId)
{
    var result = await _ticketService.GetTicketDetailById(ticketId, userId);
    return Ok(result);
}
```

**After**
```csharp
[HttpGet("detail")]
public async Task<IActionResult> GetTicketDetail([FromQuery] int ticketId)
{
    // Claims에서 userId 추출 (로그인하지 않은 경우 null)
    var userId = User.GetUserId();
    var result = await _ticketService.GetTicketDetailById(ticketId, userId);
    return Ok(result);
}
```

---

## 생성/수정된 파일 목록

### 생성된 파일 (13개)

#### Config
- `Config/JwtSettings.cs`

#### Services
- `Services/Token/ITokenService.cs`
- `Services/Token/TokenService.cs`

#### Repository
- `Repository/Token/IRefreshTokenRepository.cs`
- `Repository/Token/RefreshTokenRepository.cs`

#### DTO
- `DTO/Auth/TokenResponseDto.cs`
- `DTO/Auth/RefreshTokenReqDto.cs`

#### DBModel
- `DBModel/RefreshToken.cs`

#### Common
- `Common/ClaimsExtensions.cs`

#### Migrations
- `Migrations/[Timestamp]_AddRefreshTokenTable.cs` (생성되었으나 수동 실행)

### 수정된 파일 (9개)

#### 프로젝트 설정
- `TicketPlatFormServer.csproj` - NuGet 패키지 추가
- `appsettings.json` - JWT 설정 추가
- `Program.cs` - JWT 미들웨어 등록

#### Database
- `Repository/TicketContext.cs` - RefreshToken DbSet 추가

#### Controllers
- `Controllers/AuthController.cs` - Refresh, Logout 엔드포인트 추가
- `Controllers/FavoriteController.cs` - [Authorize] 적용, Claims 사용
- `Controllers/TicketController.cs` - userId 파라미터 제거, Claims 사용

#### Services
- `Services/User/UserService.cs` - Token 생성 로직 추가

#### DTO
- `DTO/User/LoginUserRespDto.cs` - Token 필드 추가
- `DTO/Favorite/FavoriteToggleReqDto.cs` - [JsonIgnore] 추가

---

## 데이터베이스 변경사항

### 새로운 테이블: refresh_tokens

```sql
CREATE TABLE refresh_tokens (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    token VARCHAR(500) NOT NULL UNIQUE,
    expiry_date DATETIME NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_revoked BOOLEAN DEFAULT FALSE,
    revoked_at DATETIME NULL,
    replaced_by_token VARCHAR(500) NULL,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_user_token (user_id, token),
    INDEX idx_expiry (expiry_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='Refresh Token 저장 테이블';
```

### 테이블 구조

| 컬럼 | 타입 | 설명 |
|------|------|------|
| id | INT | Primary Key |
| user_id | INT | 사용자 ID (FK) |
| token | VARCHAR(500) | Refresh Token (UUID, Unique) |
| expiry_date | DATETIME | 만료 일시 (UTC) |
| created_at | TIMESTAMP | 생성 일시 |
| is_revoked | BOOLEAN | 무효화 여부 |
| revoked_at | DATETIME | 무효화 일시 |
| replaced_by_token | VARCHAR(500) | 대체 Token (Token Rotation) |

---

## 보안 개선사항

### 1. 사용자 인증 강제

**Before**: 모든 API가 공개 상태
```bash
# 인증 없이 접근 가능 (보안 취약)
GET /api/favorites/tickets?userId=123
```

**After**: 인증이 필요한 API는 [Authorize] 적용
```bash
# JWT Token 필수
GET /api/favorites/tickets
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### 2. userId 위조 방지

**Before**: 클라이언트가 userId 전송 (위조 가능)
```json
POST /api/favorites/tickets
{
  "userId": 999,  // 타인의 ID로 요청 가능 (심각한 보안 취약점)
  "ticketId": 123
}
```

**After**: 서버에서 JWT Claims로부터 userId 추출 (위조 불가능)
```json
POST /api/favorites/tickets
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
{
  "ticketId": 123  // userId는 서버에서 자동 추출
}
```

### 3. Token Rotation 구현

Refresh Token 사용 시 이전 Token을 즉시 무효화하여 보안 강화

```csharp
// 1. 새 Token 생성
var newToken = await _tokenService.GenerateTokensAsync(user, 7);

// 2. 이전 Token 무효화
await _refreshTokenRepo.RevokeRefreshTokenAsync(oldToken, newToken.RefreshToken);

// 3. 새 Token 저장
await _refreshTokenRepo.SaveRefreshTokenAsync(newRefreshToken);
```

### 4. Token 만료 시간 관리

- **Access Token**: 15분 (짧은 수명으로 보안 강화)
- **Refresh Token**: 7일 (긴 수명으로 사용자 편의성 확보)
- **ClockSkew**: 0초 (엄격한 만료 시간 검증)

### 5. Claims 기반 사용자 정보 추출

```csharp
// Token에서 자동으로 사용자 정보 추출
var userId = User.GetUserId();
var email = User.GetEmail();
var role = User.GetRole();
var provider = User.GetProvider();
```

---

## 테스트 가이드

### 1. 로그인 및 Token 발급

```bash
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "password123"
  }'
```

**응답 예시**
```json
{
  "message": "로그인 성공",
  "data": {
    "id": 1,
    "email": "test@example.com",
    "role": "user",
    "provider": "email",
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "550e8400-e29b-41d4-a716-446655440000",
    "expiresIn": 900,
    "tokenType": "Bearer",
    "expiresAt": "2026-01-08T11:15:00Z"
  },
  "statusCode": 200
}
```

### 2. 보호된 API 접근

```bash
curl -X GET http://localhost:5000/api/favorites/tickets \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### 3. Token 갱신

```bash
curl -X POST http://localhost:5000/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "550e8400-e29b-41d4-a716-446655440000"
  }'
```

### 4. 로그아웃

```bash
curl -X POST http://localhost:5000/auth/logout \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "550e8400-e29b-41d4-a716-446655440000"
  }'
```

### 5. 인증 없이 접근 시도 (401 에러 확인)

```bash
curl -X GET http://localhost:5000/api/favorites/tickets
# 응답: 401 Unauthorized
```

---

## 향후 개선 사항

### Phase 4: Role 기반 접근 제어 (RBAC)

현재는 로그인한 사용자만 구분하고 있으나, 향후 Role 기반 접근 제어를 추가할 수 있습니다.

```csharp
// Admin 전용 API
[Authorize(Roles = "admin")]
[HttpPost("admin/users/{id}/ban")]
public async Task<IActionResult> BanUser([FromRoute] int id)
{
    // Admin만 접근 가능
}

// 일반 사용자 + Admin
[Authorize(Roles = "user,admin")]
[HttpGet("api/users/profile")]
public async Task<IActionResult> GetProfile()
{
    // User와 Admin 모두 접근 가능
}
```

### 추가 보안 강화

1. **HTTPS 강제**
   ```csharp
   app.UseHttpsRedirection();
   app.Use(async (context, next) => {
       context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000");
       await next();
   });
   ```

2. **CORS 설정**
   ```csharp
   builder.Services.AddCors(options => {
       options.AddPolicy("AllowTrustedOrigins", policy => {
           policy.WithOrigins("https://yourdomain.com")
                 .AllowAnyMethod()
                 .AllowAnyHeader()
                 .AllowCredentials();
       });
   });
   ```

3. **보안 헤더 추가**
   ```csharp
   app.Use(async (context, next) => {
       context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
       context.Response.Headers.Add("X-Frame-Options", "DENY");
       context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
       await next();
   });
   ```

4. **Refresh Token 정리 작업**
   ```csharp
   // 주기적으로 만료된 Token 삭제
   public async Task CleanupExpiredTokens()
   {
       await _refreshTokenRepo.RemoveExpiredTokensAsync();
   }
   ```

5. **IP 기반 접근 제어**
   - 의심스러운 IP 차단
   - Rate Limiting 구현

6. **Token Blacklist**
   - Redis를 활용한 무효화된 Access Token 목록 관리

---

## 빌드 결과

```
빌드했습니다.
경고 13개 (기존 nullable 경고, 코드 품질에 영향 없음)
오류 0개

경과 시간: 00:00:01.36
```

---

## 결론

JWT 인증 시스템 구현을 통해 다음과 같은 성과를 달성했습니다:

✅ **보안 강화**
- userId 위조 불가능
- 사용자 인증 강제
- Token 기반 세션 관리

✅ **확장성**
- Claims 기반 사용자 정보 추출
- Role 기반 접근 제어 준비 완료
- Token Rotation 패턴 구현

✅ **안정성**
- 0개의 빌드 에러
- 기존 코드와의 호환성 유지
- 체계적인 에러 처리

이제 티켓 플랫폼 서버는 안전하고 확장 가능한 인증 시스템을 갖추게 되었습니다.

---

**작성자**: Claude Sonnet 4.5
**검토 필요 사항**: 프로덕션 배포 전 Secret Key를 환경 변수로 관리할 것을 권장합니다.
