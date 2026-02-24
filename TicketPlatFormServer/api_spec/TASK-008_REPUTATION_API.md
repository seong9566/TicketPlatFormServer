# TASK-008 Reputation API Update

## Added Endpoints

### POST /api/reputations

- Auth required
- Request body
  - `transactionId: long`
  - `score: int` (1..5)
- Response
  - `ApiResponse<long>`
  - `statusCode: 201`
  - `data: reputationId`

### GET /api/reputations/check/{transactionId}

- Auth required
- Response
  - `ApiResponse<ReputationCheckRespDto>`
  - `statusCode: 200`

### GET /api/users/{userId}/reputations?page=1&size=20

- Public endpoint
- Response
  - `ApiResponse<ReputationListRespDto>`
  - `statusCode: 200`

## Added DTOs

- `CreateReputationReqDto`
  - `transactionId: long`
  - `score: int`
- `ReputationCheckRespDto`
  - `canReview: bool`
  - `hasReviewed: bool`
  - `reviewDeadline?: datetime`
- `ReputationRespDto`
  - `id: long`
  - `reviewerNickname: string`
  - `reviewerProfileImageUrl?: string`
  - `score: int`
  - `createdAt: datetime`
- `ReputationListRespDto`
  - `items: ReputationRespDto[]`
  - `totalCount: int`
  - `averageRating?: float`

## Existing DTO field updates

- `UserProfileDto`
  - `averageRating?: float`
  - `reviewCount: int`
- `ChatRoomDetailRespDto`
  - `canWriteReview: bool`
  - `hasReviewedSeller: bool`

## Notification update

- Added notification type code: `REVIEW_REQUEST`
- Emitted on both manual purchase confirm and automatic purchase confirm
