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

## Plan (Step by Step)
1) Add Supabase config model
   - New config class: `TicketPlatFormServer/Config/SupabaseStorageSettings.cs`
   - Fields: `ProjectUrl`, `ServiceRoleKey`, `BucketName`, `SignedUrlExpirySeconds`, `MaxFileSizeMB`, `AllowedExtensions`.

2) Add appsettings entries
   - Add a new `SupabaseStorage` section in `TicketPlatFormServer/appsettings.json`.
   - Keep S3 settings for fallback during rollout (optional).

3) Implement Supabase uploader
   - Option A (replace): update `FileUploadService` to use Supabase Storage REST API.
   - Option B (adapter): introduce `IStorageUploader` with `S3StorageUploader` and `SupabaseStorageUploader`, and inject the chosen implementation.
   - Use HTTP POST to Supabase Storage: `/storage/v1/object/{bucket}/{path}`.
   - Required headers (server-side only):
     - `Authorization: Bearer {ServiceRoleKey}`
     - `apikey: {ServiceRoleKey}`
     - `Content-Type: {file.ContentType}`
     - Optional: `x-upsert: true` (if overwrite is allowed).

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

8) Signed URL refresh on reads
   - When fetching chat messages, if stored value is a key, re-issue signed URL per message.
   - A lightweight cache can be added to reduce frequent signing on hot rooms.

9) Update docs
   - Document private bucket + signed URL behavior in `TicketPlatFormServer/api_spec/chat_api.md`.

## Security Notes
- Do NOT expose `ServiceRoleKey` to clients.
- Limit bucket to images only.
- Validate content-type and extension server-side.
 - Signed URL expiry should be short (e.g., 5~30 minutes).

## Testing Plan
- Upload small image (success).
- Upload blocked extension (reject).
- Upload over max size (reject).
- Verify signed URL works in chat response.
- Verify signed URL refresh on message list/room detail.
- Delete by key works.

## Rollout
- Stage: enable Supabase in dev.
- Prod: switch config to Supabase and monitor upload failures.
- Optional feature flag: choose provider by config key.

## Backout
- Revert config to S3 or swap DI binding back to S3 uploader.
