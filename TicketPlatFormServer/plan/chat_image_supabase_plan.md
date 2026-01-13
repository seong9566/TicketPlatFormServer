# Chat Image Upload (Server -> Supabase) Plan

## Goal
Replace S3-based chat image upload with Supabase Storage, keeping the existing API contract:
- Client still posts multipart form data to `POST /api/chat/messages`.
- Server uploads the file and returns `imageUrl` in the response.
 - **Private bucket + signed URL** policy is used.
 - Store **object key** (path) in DB, not the full URL.

## Non-Goals
- Client direct uploads.
- Migrating existing images (no data to migrate).
- Changing chat message schema.

## Current State (Key Files)
- `TicketPlatFormServer/Services/FileUpload/FileUploadService.cs` uses AWS S3 SDK.
- `TicketPlatFormServer/Config/AwsS3Settings.cs` + `appsettings.json` hold S3 config.
- Chat message upload uses `IFileUploadService.UploadChatImageAsync`.

## Architecture Decision
**Option B (Adapter Pattern) 채택**
- `IStorageUploader` 인터페이스 도입
- `S3StorageUploader`, `SupabaseStorageUploader` 구현체 분리
- DI를 통해 provider 교체 가능
- 장점: 테스트 용이성, 추후 provider 교체 용이, 롤백 간편

## Plan (Step by Step)
1) Add Supabase config model
   - New config class: `TicketPlatFormServer/Config/SupabaseStorageSettings.cs`
   - Fields: `ProjectUrl`, `ServiceRoleKey`, `BucketName`, `MaxFileSizeMB`, `AllowedExtensions`.
   - **Signed URL Expiry 분리**:
     - `UploadSignedUrlExpirySec`: 업로드 직후 반환용 (긴 시간, 예: 3600초)
     - `ReadSignedUrlExpirySec`: 메시지 조회 시 재발급용 (짧은 시간, 예: 1800초)

2) Add appsettings entries
   - Add a new `SupabaseStorage` section in `TicketPlatFormServer/appsettings.json`.
   - Keep S3 settings for fallback during rollout (optional).

3) Implement Storage Adapter Pattern
   - **Interface**: `IStorageUploader`
     ```csharp
     public interface IStorageUploader
     {
         Task<string> UploadAsync(Stream stream, string objectKey, string contentType);
         Task<string> GetSignedUrlAsync(string objectKey, int expirySec);
         Task<List<string>> GetSignedUrlsBatchAsync(List<string> objectKeys, int expirySec);
         Task DeleteAsync(string objectKey);
     }
     ```
   - **구현체**: `SupabaseStorageUploader`
     - Use HTTP POST to Supabase Storage: `/storage/v1/object/{bucket}/{path}`
     - Required headers (server-side only):
       - `Authorization: Bearer {ServiceRoleKey}`
       - `apikey: {ServiceRoleKey}`
       - `Content-Type: {file.ContentType}`
     - `x-upsert: false` (중복 키 시 에러 반환, 덮어쓰기 방지)
   - **구현체**: `S3StorageUploader` (기존 로직 래핑, fallback용)

4) Preserve validation logic
   - Keep file size/extension checks in `FileUploadService`.
   - Generate file key: `chat/{roomId}/{userId}_{timestamp}_{guid}{ext}`.
   - Persist **object key** (path) to DB, not a public URL.

5) Return a signed URL (private bucket)
   - Generate signed URL via Supabase REST:
     - `POST /storage/v1/object/sign/{bucket}/{path}`
     - Body: `{ "expiresIn": <SignedUrlExpirySeconds> }`
   - Return signed URL in API response (`imageUrl`), **store only the object key**.

6) Delete file support
   - Implement delete via Supabase Storage REST: `DELETE /storage/v1/object/{bucket}/{path}`.
   - `DeleteFileAsync` should accept object key or extract key from signed URL (if still passed).

7) Wire up DI
   - Register new settings and uploader in `Program.cs`.
   - Remove AWS SDK registration if fully migrated.

8) Signed URL refresh on reads (성능 최적화)
   - When fetching chat messages, if stored value is a key, re-issue signed URL.
   - **N+1 문제 해결 전략**:
     - Supabase Batch Sign API 사용: `POST /storage/v1/object/sign/{bucket}` with body `{ "paths": [...], "expiresIn": N }`
     - 한 번의 API 호출로 여러 object key에 대한 signed URL 일괄 생성
   - **캐싱 전략**:
     - `IMemoryCache` 사용 (Redis 도입 전까지)
     - Cache key: `signed_url:{objectKey}`
     - TTL: `ReadSignedUrlExpirySec - 60` (만료 1분 전 갱신 유도)
     - 캐시 히트 시 Supabase API 호출 생략
   - **클라이언트 측 만료 대응**:
     - 응답에 `imageUrlExpiresAt` (Unix timestamp) 포함
     - 클라이언트는 만료 전 `GET /api/chat/messages/{messageId}/image-url` 호출하여 재발급
     - 또는 만료된 이미지 로드 실패 시 자동 재요청

9) Update docs
   - Document private bucket + signed URL behavior in `TicketPlatFormServer/api_spec/chat_api.md`.

## Error Handling
- **Retry 정책**:
  - Supabase API 호출 실패 시 Polly 라이브러리 사용
  - Exponential backoff: 1초 → 2초 → 4초 (최대 3회 재시도)
  - 재시도 대상: 5xx 에러, 타임아웃, 네트워크 에러
  - 재시도 제외: 4xx 에러 (클라이언트 에러는 즉시 실패)
- **Rate Limiting 대응**:
  - 429 응답 시 `Retry-After` 헤더 존중
  - Circuit breaker 패턴 적용 (연속 5회 실패 시 30초간 차단)
- **Timeout 설정**:
  - Upload: 30초 (파일 크기에 따라 조정 가능)
  - Sign URL: 5초
  - Delete: 10초
- **Fallback**:
  - Supabase 장애 시 에러 로깅 후 클라이언트에 명확한 에러 메시지 반환
  - 선택적: S3 fallback 활성화 (config flag로 제어)

## Security Notes
- Do NOT expose `ServiceRoleKey` to clients.
- Limit bucket to images only.
- **Content-Type 검증 강화**:
  - 확장자 검증 + Magic bytes 검증 병행
  - 허용 Magic bytes: `FFD8FF` (JPEG), `89504E47` (PNG), `47494638` (GIF), `52494646` (WEBP)
  - Content-Type과 Magic bytes 불일치 시 업로드 거부
- Signed URL expiry 설정:
  - 업로드 직후 반환: 1시간 (사용자가 바로 확인하도록)
  - 메시지 조회 시: 30분 (캐싱과 함께 사용)

## Observability (Logging & Metrics)
### Structured Logging
- **Upload 작업**:
  - `[INFO] ChatImage.Upload` - `{ roomId, userId, objectKey, fileSize, contentType, durationMs }`
  - `[ERROR] ChatImage.Upload.Failed` - `{ roomId, userId, errorCode, errorMessage }`
- **Sign URL 작업**:
  - `[DEBUG] ChatImage.SignUrl` - `{ objectKey, expirySec, cached: bool }`
  - `[WARN] ChatImage.SignUrl.CacheMiss` - `{ objectKey, reason }`
- **Delete 작업**:
  - `[INFO] ChatImage.Delete` - `{ objectKey, durationMs }`

### Metrics (Prometheus/OpenTelemetry 호환)
| Metric Name | Type | Labels | Description |
|-------------|------|--------|-------------|
| `chat_image_upload_total` | Counter | `status` (success/failure) | 업로드 시도 횟수 |
| `chat_image_upload_duration_ms` | Histogram | - | 업로드 소요 시간 |
| `chat_image_upload_size_bytes` | Histogram | - | 업로드 파일 크기 분포 |
| `chat_image_signurl_total` | Counter | `cached` (true/false) | Sign URL 요청 횟수 |
| `chat_image_signurl_cache_hit_ratio` | Gauge | - | 캐시 히트율 |
| `supabase_api_errors_total` | Counter | `operation`, `status_code` | Supabase API 에러 횟수 |

## Testing Plan
- Upload small image (success).
- Upload blocked extension (reject).
- Upload over max size (reject).
- **Upload with invalid magic bytes (reject)**.
- Verify signed URL works in chat response.
- Verify signed URL refresh on message list/room detail.
- **Verify batch sign URL performance (N images)**.
- **Verify cache hit/miss behavior**.
- Delete by key works.
- **Verify retry on transient failures**.
- **Verify circuit breaker activation**.

## Rollout
- Stage: enable Supabase in dev.
- Prod: switch config to Supabase and monitor upload failures.
- Optional feature flag: choose provider by config key.

## Backout
- Revert config to S3 or swap DI binding back to S3 uploader.
