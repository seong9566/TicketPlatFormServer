using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.Config;
using TicketPlatFormServer.DBModel;
using TicketPlatFormServer.DTO.Chat;
using TicketPlatFormServer.DTO.Payment;
using TicketPlatFormServer.Hubs;
using TicketPlatFormServer.Repository;
using TicketPlatFormServer.Repository.Chat;
using TicketPlatFormServer.Repository.Payment;
using TicketPlatFormServer.Repository.Transactions;
using TicketPlatFormServer.Services.Common;
using TicketPlatFormServer.Services.Notification;

namespace TicketPlatFormServer.Services.Payment;

/// <summary>
/// 결제 비즈니스 로직 서비스 구현체
/// Primary Constructor 패턴 사용
/// </summary>
public class PaymentService(
    ITossPaymentsService tossPaymentsService,
    IPaymentRepository paymentRepository,
    ITransactionRepository transactionRepository,
    IChatRepository chatRepository,
    TicketContext context,
    TossPaymentsSettings settings,
    EncryptionService encryptionService,
    INotificationService notificationService,
    IHubContext<ChatHub> hubContext,
    ILogger<PaymentService> logger) : IPaymentService
{
    /// <summary>
    /// 결제 요청 준비 (OrderId 생성)
    /// </summary>
    public async Task<PaymentRequestResponseDto> InitiatePaymentAsync(PaymentRequestDto request, int userId)
    {
        logger.LogInformation("[PaymentService.InitiatePaymentAsync] TransactionId: {TransactionId}, UserId: {UserId}",
            request.TransactionId, userId);

        // 1. Transaction 존재 및 소유권 검증
        var transaction = await transactionRepository.GetTransactionById(request.TransactionId);
        if (transaction == null)
        {
            throw new AppException("거래를 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        if (!string.Equals(transaction.Status.Code, "pending_payment", StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException("결제 요청 상태가 올바르지 않습니다.", HttpStatusCode.BadRequest);
        }

        if (transaction.ReservationExpiresAt != null && transaction.ReservationExpiresAt < DateTime.UtcNow)
        {
            throw new AppException("결제 요청이 만료되었습니다.", HttpStatusCode.BadRequest);
        }

        if (transaction.BuyerId != userId)
        {
            throw new AppException("해당 거래의 구매자만 결제를 요청할 수 있습니다.", HttpStatusCode.Forbidden);
        }

        // 2. 거래 금액 검증 (TransactionItems 합계)
        var totalAmount = await context.TransactionItems
            .Where(ti => ti.TransactionId == request.TransactionId)
            .SumAsync(ti => ti.TotalPrice);

        if (request.Amount != totalAmount)
        {
            throw new AppException("결제 금액이 거래 금액과 일치하지 않습니다.", HttpStatusCode.BadRequest);
        }

        var paymentPreview = await transactionRepository.GetPaymentPreviewAsync(request.TransactionId, userId);

        // 3. OrderId 생성: TXN_{TransactionId}_{Guid}
        var orderId = $"TXN_{request.TransactionId}_{Guid.NewGuid():N}";

        logger.LogInformation("[PaymentService.InitiatePaymentAsync] OrderId generated: {OrderId}", orderId);

        return new PaymentRequestResponseDto
        {
            OrderId = orderId,
            Amount = request.Amount,
            OrderName = request.OrderName,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            SuccessUrl = settings.SuccessUrl,
            FailUrl = settings.FailUrl,
            ClientKey = settings.ClientKey,
            TicketInfo = paymentPreview == null
                ? null
                : new PaymentTicketInfoDto
                {
                    TicketId = paymentPreview.TicketId,
                    SeatInfo = paymentPreview.SeatInfo,
                    Quantity = paymentPreview.Quantity,
                    UnitPrice = paymentPreview.UnitPrice,
                    TotalAmount = paymentPreview.TotalAmount,
                    ThumbnailUrl = paymentPreview.ThumbnailUrl
                },
            EventInfo = paymentPreview == null
                ? null
                : new PaymentEventInfoDto
                {
                    EventId = paymentPreview.EventId,
                    Title = paymentPreview.EventTitle,
                    EventDateTime = paymentPreview.EventDateTime,
                    VenueName = paymentPreview.VenueName
                }
        };
    }

    /// <summary>
    /// 결제 승인 처리 (Toss API 호출 + DB 저장)
    /// </summary>
    public async Task<TossPaymentResponseDto> ConfirmPaymentAsync(PaymentConfirmRequestDto request)
    {
        logger.LogInformation("[PaymentService.ConfirmPaymentAsync] OrderId: {OrderId}, PaymentKey: {PaymentKey}",
            request.OrderId, request.PaymentKey);

        // 1. OrderId에서 TransactionId 추출
        if (!TryExtractTransactionId(request.OrderId, out var transactionId))
        {
            throw new AppException("유효하지 않은 OrderId 형식입니다.", HttpStatusCode.BadRequest);
        }

        // 2. 중복 결제 방지 (Idempotency)
        var existingPayment = await paymentRepository.GetPaymentByOrderIdAsync(request.OrderId);
        if (existingPayment != null)
        {
            logger.LogWarning("[PaymentService.ConfirmPaymentAsync] 중복 결제 시도: {OrderId}", request.OrderId);

            // 기존 결제 정보 반환 (토스 API 재호출 대신)
            return await tossPaymentsService.GetPaymentAsync(existingPayment.PaymentKey!);
        }

        // 3. Transaction 조회 및 검증
        var transaction = await transactionRepository.GetTransactionById(transactionId);
        if (transaction == null)
        {
            throw new AppException("거래를 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        if (!string.Equals(transaction.Status.Code, "pending_payment", StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException("결제 요청 상태가 올바르지 않습니다.", HttpStatusCode.BadRequest);
        }

        if (transaction.ReservationExpiresAt != null && transaction.ReservationExpiresAt < DateTime.UtcNow)
        {
            throw new AppException("결제 요청이 만료되었습니다.", HttpStatusCode.BadRequest);
        }

        // 4. Toss API 승인 호출
        TossPaymentResponseDto tossResponse;
        try
        {
            tossResponse = await tossPaymentsService.ConfirmPaymentAsync(
                request.PaymentKey, request.OrderId, request.Amount);
        }
        catch (AppException ex)
        {
            logger.LogError(ex, "[PaymentService.ConfirmPaymentAsync] Toss API 승인 실패");
            throw;
        }
        logger.LogInformation("[Toss성공 Response {tossResponse}]",tossResponse);

        // 5. 금액 검증
        if (tossResponse.TotalAmount != request.Amount)
        {
            throw new AppException("결제 금액이 일치하지 않습니다.", HttpStatusCode.BadRequest);
        }

        // 6. DB 트랜잭션 시작
        await using var dbTransaction = await context.Database.BeginTransactionAsync();
        try
        {
            // 6-1. PaymentMethod 조회
            var paymentMethod = await GetOrCreatePaymentMethodAsync(tossResponse.Method);

            // 6-2. PaymentStatus 조회 (paid)
            var paymentStatus = await paymentRepository.GetPaymentStatusByCodeAsync("paid");
            if (paymentStatus == null)
            {
                throw new AppException("결제 상태 코드를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
            }

            // 6-3. Payment 레코드 생성
            var payment = new DBModel.Payment
            {
                TransactionId = transactionId,
                PgProvider = "toss",
                MerchantId = tossResponse.MId,
                ApiVersion = tossResponse.Version,
                Country = tossResponse.Country ?? "KR",
                PaymentKey = tossResponse.PaymentKey,
                OrderId = tossResponse.OrderId,
                Amount = (ulong)tossResponse.TotalAmount,
                MethodId = paymentMethod.Id,
                PaidAt = string.IsNullOrEmpty(tossResponse.ApprovedAt)
                    ? DateTime.UtcNow
                    : DateTime.Parse(tossResponse.ApprovedAt).ToUniversalTime(),
                StatusId = paymentStatus.Id,
                UseEscrow = tossResponse.UseEscrow,
                IsPartialCancelable = tossResponse.IsPartialCancelable,
                PaymentType = tossResponse.Type,
                LastTransactionKey = tossResponse.LastTransactionKey,
                CultureExpense = tossResponse.CultureExpense,
                Metadata = tossResponse.Metadata != null ? JsonSerializer.Serialize(tossResponse.Metadata) : null,
                DiscountInfo = tossResponse.Discount != null ? JsonSerializer.Serialize(tossResponse.Discount) : null
            };

            await paymentRepository.CreatePaymentAsync(payment);

            // 6-3-1. 카드 결제 상세 정보 저장
            if (tossResponse.Card != null)
            {
                var cardCompany = tossResponse.Card.Company ?? tossResponse.EasyPay?.Provider ?? "UNKNOWN";

                var cardDetail = new PaymentCardDetail
                {
                    PaymentId = payment.Id,
                    Company = cardCompany,
                    CardNumber = tossResponse.Card.Number,
                    InstallmentPlanMonths = tossResponse.Card.InstallmentPlanMonths,
                    ApproveNo = tossResponse.Card.ApproveNo,
                    CardType = tossResponse.Card.CardType,
                    OwnerType = tossResponse.Card.OwnerType,
                    AcquireStatus = tossResponse.Card.AcquireStatus,
                    IsInterestFree = tossResponse.Card.IsInterestFree,
                    IssuerCode = tossResponse.Card.IssuerCode,
                    AcquirerCode = tossResponse.Card.AcquirerCode,
                    InterestPayer = tossResponse.Card.InterestPayer,
                    UseCardPoint = tossResponse.Card.UseCardPoint,
                    Amount = (ulong)tossResponse.Card.Amount,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await paymentRepository.CreateCardDetailAsync(cardDetail);
            }

            // 6-3-2. 가상계좌 상세 정보 저장
            if (tossResponse.VirtualAccount != null)
            {
                var vaDetail = new PaymentVirtualAccountDetail
                {
                    PaymentId = payment.Id,
                    AccountNumber = tossResponse.VirtualAccount.AccountNumber,
                    BankCode = tossResponse.VirtualAccount.BankCode,
                    CustomerName = tossResponse.VirtualAccount.CustomerName,
                    DueDate = DateTime.Parse(tossResponse.VirtualAccount.DueDate),
                    RefundStatus = tossResponse.VirtualAccount.RefundStatus,
                    Expired = tossResponse.VirtualAccount.Expired,
                    SettlementStatus = tossResponse.VirtualAccount.SettlementStatus,
                    AccountType = tossResponse.VirtualAccount.AccountType,
                    RefundReceiveAccount = encryptionService.EncryptNullable(
                        tossResponse.VirtualAccount.RefundReceiveAccount != null
                            ? JsonSerializer.Serialize(tossResponse.VirtualAccount.RefundReceiveAccount)
                            : null),
                    Secret = encryptionService.EncryptNullable(tossResponse.Secret),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await paymentRepository.CreateVirtualAccountDetailAsync(vaDetail);
            }

            // 6-3-3. 간편결제 상세 정보 저장
            if (tossResponse.EasyPay != null)
            {
                var easyPayDetail = new PaymentEasyPayDetail
                {
                    PaymentId = payment.Id,
                    Provider = tossResponse.EasyPay.Provider,
                    Amount = (ulong)tossResponse.EasyPay.Amount,
                    DiscountAmount = (ulong)tossResponse.EasyPay.DiscountAmount,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await paymentRepository.CreateEasyPayDetailAsync(easyPayDetail);
            }

            // 6-3-4. 현금영수증 정보 저장
            if (tossResponse.CashReceipt != null)
            {
                var cashReceipt = new PaymentCashReceipt
                {
                    PaymentId = payment.Id,
                    ReceiptType = tossResponse.CashReceipt.Type,
                    ReceiptKey = tossResponse.CashReceipt.ReceiptKey,
                    IssueNumber = tossResponse.CashReceipt.IssueNumber,
                    ReceiptUrl = tossResponse.CashReceipt.ReceiptUrl,
                    Amount = (ulong)tossResponse.CashReceipt.Amount,
                    TaxFreeAmount = (ulong)tossResponse.CashReceipt.TaxFreeAmount,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await paymentRepository.CreateCashReceiptAsync(cashReceipt);
            }

            // 6-3-5. 결제 거래 이벤트 저장
            var paymentTransaction = new PaymentTransaction
            {
                PaymentId = payment.Id,
                TransactionKey = tossResponse.PaymentKey, // 승인 시에는 PaymentKey가 TransactionKey
                TransactionType = "PAYMENT",
                Amount = (ulong)tossResponse.TotalAmount,
                BalanceAmount = (ulong)tossResponse.BalanceAmount,
                TaxFreeAmount = (ulong)tossResponse.TaxFreeAmount,
                Currency = tossResponse.Currency ?? "KRW",
                Status = "DONE",
                Reason = null,
                TossResponse = encryptionService.EncryptNullable(JsonSerializer.Serialize(tossResponse)),
                EventAt = string.IsNullOrEmpty(tossResponse.ApprovedAt)
                    ? DateTime.UtcNow
                    : DateTime.Parse(tossResponse.ApprovedAt).ToUniversalTime(),
                CreatedAt = DateTime.UtcNow
            };
            await paymentRepository.CreateTransactionAsync(paymentTransaction);

            // 6-4. Fee 계산 (3.5%)
            var feeAmount = (int)(tossResponse.TotalAmount * (settings.EscrowFeePercentage / 100m));
            var sellerAmount = tossResponse.TotalAmount - feeAmount;

            // 6-5. EscrowStatus 조회 (holding)
            var escrowStatus = await paymentRepository.GetEscrowStatusByCodeAsync("holding");
            if (escrowStatus == null)
            {
                throw new AppException("에스크로 상태 코드를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
            }

            // 6-6. Escrow 레코드 생성
            var escrow = new Escrow
            {
                TransactionId = transactionId,
                Amount = tossResponse.TotalAmount,
                FeeAmount = feeAmount,
                SellerAmount = sellerAmount,
                StatusId = escrowStatus.Id
            };

            await paymentRepository.CreateEscrowAsync(escrow);

            // 6-7. Transaction.StatusId 업데이트 (paid)
            var transactionStatus = await paymentRepository.GetTransactionStatusByCodeAsync("paid");
            if (transactionStatus == null)
            {
                throw new AppException("거래 상태 코드를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
            }

            await transactionRepository.UpdateTransactionStatusAsync(transactionId, transactionStatus.Id);

            // 6-8. 커밋
            await dbTransaction.CommitAsync();

            await TryCreatePaymentSuccessMessageAsync(transactionId);

            logger.LogInformation(
                "[PaymentService.ConfirmPaymentAsync] 결제 완료 - PaymentId: {PaymentId}, EscrowId: {EscrowId}, Amount: {Amount}, Fee: {FeeAmount}",
                payment.Id, escrow.Id, tossResponse.TotalAmount, feeAmount);

            return tossResponse;
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            logger.LogError(ex, "[PaymentService.ConfirmPaymentAsync] DB 트랜잭션 실패");
            throw new AppException("결제 처리 중 오류가 발생했습니다.", HttpStatusCode.InternalServerError, ex);
        }
    }

    /// <summary>
    /// 에스크로 해제 (구매 확정 시)
    /// </summary>
    public async Task ReleaseEscrowAsync(long transactionId)
    {
        logger.LogInformation("[PaymentService.ReleaseEscrowAsync] TransactionId: {TransactionId}", transactionId);

        // 1. Escrow 조회
        var escrow = await paymentRepository.GetEscrowByTransactionIdAsync(transactionId);
        if (escrow == null)
        {
            throw new AppException("에스크로를 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        // 2. 이미 해제된 경우
        if (escrow.ReleasedAt != null)
        {
            var transaction = await context.Transactions
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction != null)
            {
                var confirmedStatus = await paymentRepository.GetTransactionStatusByCodeAsync("confirmed");
                if (confirmedStatus != null)
                {
                    var needsSave = false;

                    if (transaction.StatusId != confirmedStatus.Id)
                    {
                        transaction.StatusId = confirmedStatus.Id;
                        needsSave = true;
                    }

                    if (transaction.ConfirmedAt == null)
                    {
                        transaction.ConfirmedAt = escrow.ReleasedAt ?? DateTime.UtcNow;
                        needsSave = true;
                    }

                    if (needsSave)
                    {
                        await context.SaveChangesAsync();
                    }
                }
            }

            logger.LogWarning("[PaymentService.ReleaseEscrowAsync] 이미 해제된 에스크로: {EscrowId}", escrow.Id);
            return;
        }

        var holdingEscrowStatus = await paymentRepository.GetEscrowStatusByCodeAsync("holding");
        if (holdingEscrowStatus == null)
        {
            throw new AppException("에스크로 상태 코드를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
        }

        if (escrow.StatusId != holdingEscrowStatus.Id)
        {
            throw new AppException("에스크로가 해제 가능한 상태가 아닙니다.", HttpStatusCode.Conflict);
        }

        var transactionSnapshot = await transactionRepository.GetTransactionById(transactionId);
        if (transactionSnapshot == null)
        {
            throw new AppException("거래를 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        if (transactionSnapshot.CancelledAt != null || !string.Equals(transactionSnapshot.Status.Code, "paid", StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException("해당 거래는 구매확정 처리할 수 없는 상태입니다.", HttpStatusCode.Conflict);
        }

        // 3. DB 트랜잭션 시작
        await using var dbTransaction = await context.Database.BeginTransactionAsync();
        try
        {
            // 3-1. EscrowStatus 조회 (released)
            var escrowStatus = await paymentRepository.GetEscrowStatusByCodeAsync("released");
            if (escrowStatus == null)
            {
                throw new AppException("에스크로 상태 코드를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
            }

            // 3-2. Escrow.StatusId = released, ReleasedAt = Now
            var affectedRows = await paymentRepository.ReleaseEscrowAsync(escrow.Id, escrowStatus.Id, holdingEscrowStatus.Id, DateTime.UtcNow);
            if (affectedRows == 0)
            {
                throw new AppException("에스크로 상태가 변경되어 구매확정을 진행할 수 없습니다.", HttpStatusCode.Conflict);
            }

            // 3-3. Transaction.StatusId = confirmed, ConfirmedAt = Now
            var transactionStatus = await paymentRepository.GetTransactionStatusByCodeAsync("confirmed");
            if (transactionStatus == null)
            {
                throw new AppException("거래 상태 코드를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
            }

            await transactionRepository.UpdateTransactionStatusAsync(transactionId, transactionStatus.Id);

            // 3-4. Transaction.ConfirmedAt 업데이트 및 티켓 정보 조회
            var transaction = await context.Transactions
                .Include(t => t.TransactionItems)
                .FirstOrDefaultAsync(t => t.Id == transactionId);
            
            if (transaction == null)
            {
                throw new AppException("거래를 찾을 수 없습니다.", HttpStatusCode.NotFound);
            }
            
            transaction.ConfirmedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            var sellerProfile = await context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == (int)transaction.SellerId);
            if (sellerProfile != null)
            {
                sellerProfile.TotalTradeCount = (sellerProfile.TotalTradeCount ?? 0) + 1;
            }

            var buyerProfile = await context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == (int)transaction.BuyerId);
            if (buyerProfile != null)
            {
                buyerProfile.TotalTradeCount = (buyerProfile.TotalTradeCount ?? 0) + 1;
            }

            await context.SaveChangesAsync();

            // 3-4-1. 티켓 소유권 이전 (RemainingQuantity 감소)
            // 결제 요청 시 재고 예약이 완료된 경우 ReservedAt이 설정됨
            if (transaction.ReservedAt == null)
            {
                foreach (var item in transaction.TransactionItems)
                {
                    var ticket = await context.Tickets.FindAsync((int)item.TicketId);
                    if (ticket != null)
                    {
                        ticket.RemainingQuantity -= item.Quantity;
                        
                        if (ticket.RemainingQuantity < 0)
                        {
                            logger.LogWarning("[PaymentService.ReleaseEscrowAsync] Ticket RemainingQuantity < 0 - TicketId: {TicketId}, RemainingQuantity: {RemainingQuantity}",
                                ticket.Id, ticket.RemainingQuantity);
                            ticket.RemainingQuantity = 0;
                        }
                        
                        logger.LogInformation("[PaymentService.ReleaseEscrowAsync] Ticket 소유권 이전 - TicketId: {TicketId}, Quantity: {Quantity}, RemainingQuantity: {RemainingQuantity}",
                            ticket.Id, item.Quantity, ticket.RemainingQuantity);
                    }
                }
                await context.SaveChangesAsync();
            }

            // 3-5. Settlement 레코드 생성
            var settlementStatusPending = await paymentRepository.GetSettlementStatusByCodeAsync("pending");
            if (settlementStatusPending == null)
            {
                throw new AppException("정산 상태 코드를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
            }

            var defaultBankAccount = await context.BankAccounts
                .Where(ba => ba.UserId == transaction.SellerId && ba.Verified == true)
                .FirstOrDefaultAsync();
            
            if (defaultBankAccount == null)
            {
                logger.LogWarning("[PaymentService.ReleaseEscrowAsync] 판매자 인증된 계좌 없음 - SellerId: {SellerId}", 
                    transaction.SellerId);
            }

            var settlement = new Settlement
            {
                TransactionId = transactionId,
                SellerId = transaction.SellerId,
                Amount = escrow.Amount,
                Fee = escrow.FeeAmount,
                NetAmount = escrow.SellerAmount,
                BankAccountId = defaultBankAccount?.Id,
                StatusId = settlementStatusPending.Id,
                ScheduledAt = DateTime.UtcNow.AddDays(1),
                CreatedAt = DateTime.UtcNow
            };

            await context.Settlements.AddAsync(settlement);
            await context.SaveChangesAsync();

            logger.LogInformation("[PaymentService.ReleaseEscrowAsync] Settlement 생성 완료 - SettlementId: {SettlementId}, NetAmount: {NetAmount}",
                settlement.Id, settlement.NetAmount);

            // 3-6. 커밋
            await dbTransaction.CommitAsync();

            logger.LogInformation("[PaymentService.ReleaseEscrowAsync] 에스크로 해제 완료 - EscrowId: {EscrowId}, SellerAmount: {SellerAmount}",
                escrow.Id, escrow.SellerAmount);
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            logger.LogError(ex, "[PaymentService.ReleaseEscrowAsync] DB 트랜잭션 실패");
            throw new AppException("에스크로 해제 중 오류가 발생했습니다.", HttpStatusCode.InternalServerError, ex);
        }
    }

    private async Task TryCreatePaymentSuccessMessageAsync(long transactionId)
    {
        try
        {
            var room = await chatRepository.GetChatRoomByTransactionId(transactionId);
            if (room == null)
            {
                logger.LogWarning("[PaymentService.TryCreatePaymentSuccessMessageAsync] ChatRoom not found. TransactionId: {TransactionId}", transactionId);
                return;
            }

            var senderId = room.BuyerId;
            var message = await chatRepository.CreateMessage(
                room.Id,
                (int)senderId,
                null,
                null,
                Enum.MessageType.PAYMENT_SUCCESS);

            await chatRepository.UpdateLastMessageAt(room.Id, message.CreatedAt ?? DateTime.UtcNow);
            await chatRepository.IncrementUnreadCount(room.Id, false);

            var senderNickname = room.Buyer?.UserProfile?.Nickname ?? "Unknown";
            var senderProfileImage = room.Buyer?.UserProfile?.ProfileImageUrl;

            var messageDto = new NewMessageSignalDto
            {
                MessageId = message.Id,
                RoomId = room.Id,
                SenderId = (int)senderId,
                SenderNickname = senderNickname,
                SenderProfileImage = senderProfileImage,
                Message = null,
                Type = Enum.MessageType.PAYMENT_SUCCESS.ToString(),
                Images = null,
                CreatedAt = message.CreatedAt ?? DateTime.UtcNow
            };

            await hubContext.Clients.Group($"room_{room.Id}")
                .SendAsync("ReceiveMessage", messageDto);
            await hubContext.Clients.Group($"user_{room.BuyerId}")
                .SendAsync("ReceiveMessage", messageDto);
            await hubContext.Clients.Group($"user_{room.SellerId}")
                .SendAsync("ReceiveMessage", messageDto);

            await notificationService.CreateAndSendAsync(
                room.BuyerId,
                "PAYMENT_SUCCESS",
                "결제가 완료되었습니다",
                "결제가 정상적으로 완료되었습니다.",
                new Dictionary<string, string>
                {
                    ["type"] = "PAYMENT_SUCCESS",
                    ["transactionId"] = transactionId.ToString(),
                    ["roomId"] = room.Id.ToString()
                });

            await notificationService.CreateAndSendAsync(
                room.SellerId,
                "PAYMENT_SUCCESS",
                "결제가 완료되었습니다",
                "구매자의 결제가 완료되었습니다.",
                new Dictionary<string, string>
                {
                    ["type"] = "PAYMENT_SUCCESS",
                    ["transactionId"] = transactionId.ToString(),
                    ["roomId"] = room.Id.ToString()
                });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[PaymentService.TryCreatePaymentSuccessMessageAsync] Failed. TransactionId: {TransactionId}", transactionId);
        }
    }

    /// <summary>
    /// 결제 취소 (환불)
    /// </summary>
    public async Task<PaymentCancelResponseDto> CancelPaymentAsync(PaymentCancelRequestDto request, int userId)
    {
        logger.LogInformation("[PaymentService.CancelPaymentAsync] TransactionId: {TransactionId}, UserId: {UserId}",
            request.TransactionId, userId);

        // 1. Payment 조회
        var payment = await paymentRepository.GetPaymentByTransactionIdAsync(request.TransactionId);
        if (payment == null)
        {
            throw new AppException("결제 정보를 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        // 2. 권한 검증 (buyer/seller만 취소 가능 - 실제로는 Transaction의 BuyerId/SellerId 확인 필요)
        var transaction = await transactionRepository.GetTransactionById(request.TransactionId);
        if (transaction == null)
        {
            throw new AppException("거래를 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        if (transaction.BuyerId != userId && transaction.SellerId != userId)
        {
            throw new AppException("결제 취소 권한이 없습니다.", HttpStatusCode.Forbidden);
        }

        // 3. Toss API 취소 호출
        TossPaymentResponseDto tossResponse;
        try
        {
            tossResponse = await tossPaymentsService.CancelPaymentAsync(
                payment.PaymentKey!, request.CancelReason, request.CancelAmount);
        }
        catch (AppException ex)
        {
            logger.LogError(ex, "[PaymentService.CancelPaymentAsync] Toss API 취소 실패");
            throw;
        }

        // 4. DB 트랜잭션 시작
        await using var dbTransaction = await context.Database.BeginTransactionAsync();
        try
        {
            // 4-1. PaymentStatus 조회 (cancelled)
            var paymentStatus = await paymentRepository.GetPaymentStatusByCodeAsync("cancelled");
            if (paymentStatus == null)
            {
                throw new AppException("결제 상태 코드를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
            }

            // 4-2. Payment 상태 업데이트
            await paymentRepository.UpdatePaymentStatusAsync((long)payment.Id, paymentStatus.Id);

            // 4-3. Escrow 환불 처리
            var escrow = await paymentRepository.GetEscrowByTransactionIdAsync(request.TransactionId);
            if (escrow != null)
            {
                var escrowStatus = await paymentRepository.GetEscrowStatusByCodeAsync("refunded");
                if (escrowStatus == null)
                {
                    throw new AppException("에스크로 상태 코드를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
                }

                await paymentRepository.RefundEscrowAsync(escrow.Id, escrowStatus.Id, DateTime.UtcNow);
            }

            // 4-4. Transaction 상태 업데이트 (cancelled)
            var transactionStatus = await paymentRepository.GetTransactionStatusByCodeAsync("cancelled");
            if (transactionStatus == null)
            {
                throw new AppException("거래 상태 코드를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
            }

            await transactionRepository.UpdateTransactionStatusAsync(request.TransactionId, transactionStatus.Id);

            // 4-5. Transaction.CancelledAt 업데이트
            var txn = await context.Transactions.FindAsync(request.TransactionId);
            if (txn != null)
            {
                txn.CancelledAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }

            // 4-6. 예약 재고 복구 (전액 취소만 처리)
            var transactionWithItems = await transactionRepository.GetTransactionWithDetailsAsync(request.TransactionId);
            if (transactionWithItems != null)
            {
                var totalAmount = transactionWithItems.TransactionItems.Sum(ti => ti.TotalPrice);
                var shouldRelease = !request.CancelAmount.HasValue || request.CancelAmount.Value >= totalAmount;

                if (shouldRelease)
                {
                    foreach (var item in transactionWithItems.TransactionItems)
                    {
                        var ticket = await context.Tickets.FindAsync((int)item.TicketId);
                        if (ticket != null)
                        {
                            ticket.RemainingQuantity = Math.Min(ticket.Quantity, ticket.RemainingQuantity + item.Quantity);
                        }
                    }
                    await context.SaveChangesAsync();
                }
                else
                {
                    logger.LogWarning("[PaymentService.CancelPaymentAsync] 부분 취소로 인한 재고 복구 생략 - TransactionId: {TransactionId}, CancelAmount: {CancelAmount}",
                        request.TransactionId, request.CancelAmount);
                }
            }

            // 4-7. 커밋
            await dbTransaction.CommitAsync();

            logger.LogInformation("[PaymentService.CancelPaymentAsync] 결제 취소 완료 - PaymentId: {PaymentId}, CancelAmount: {CancelAmount}",
                payment.Id, request.CancelAmount ?? (int)payment.Amount);

            // 5. 응답 생성
            var canceledAt = tossResponse.Cancels?.LastOrDefault()?.CanceledAt;
            return new PaymentCancelResponseDto
            {
                PaymentKey = tossResponse.PaymentKey,
                OrderId = tossResponse.OrderId,
                Status = tossResponse.Status,
                CancelAmount = request.CancelAmount ?? (int)payment.Amount,
                CancelReason = request.CancelReason,
                CanceledAt = string.IsNullOrEmpty(canceledAt)
                    ? DateTime.UtcNow
                    : DateTime.Parse(canceledAt).ToUniversalTime()
            };
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            logger.LogError(ex, "[PaymentService.CancelPaymentAsync] DB 트랜잭션 실패");
            throw new AppException("결제 취소 처리 중 오류가 발생했습니다.", HttpStatusCode.InternalServerError, ex);
        }
    }

    /// <summary>
    /// Webhook 이벤트 처리
    /// </summary>
    public async Task HandleWebhookAsync(TossWebhookDto webhook)
    {
        logger.LogInformation("[PaymentService.HandleWebhookAsync] EventType: {EventType}, PaymentKey: {PaymentKey}",
            webhook.EventType, webhook.Data.PaymentKey);

        // 1. PaymentKey로 Payment 조회
        var payment = await paymentRepository.GetPaymentByPaymentKeyAsync(webhook.Data.PaymentKey);
        if (payment == null)
        {
            logger.LogWarning("[PaymentService.HandleWebhookAsync] Payment를 찾을 수 없음: {PaymentKey}", webhook.Data.PaymentKey);
            return;
        }

        // 2. 상태 변경 시 Payment.StatusId 업데이트
        if (webhook.Data.Status == "CANCELLED" || webhook.Data.Status == "PARTIAL_CANCELED")
        {
            var paymentStatus = await paymentRepository.GetPaymentStatusByCodeAsync("cancelled");
            if (paymentStatus != null)
            {
                await paymentRepository.UpdatePaymentStatusAsync((long)payment.Id, paymentStatus.Id);

                // 3. Escrow도 환불 처리
                var escrow = await paymentRepository.GetEscrowByTransactionIdAsync(payment.TransactionId);
                if (escrow != null && escrow.RefundedAt == null)
                {
                    var escrowStatus = await paymentRepository.GetEscrowStatusByCodeAsync("refunded");
                    if (escrowStatus != null)
                    {
                        await paymentRepository.RefundEscrowAsync(escrow.Id, escrowStatus.Id, DateTime.UtcNow);
                    }
                }

                logger.LogInformation("[PaymentService.HandleWebhookAsync] 결제 취소 처리 완료 - PaymentId: {PaymentId}", payment.Id);
            }
        }
    }

    /// <summary>
    /// OrderId로 결제 조회
    /// </summary>
    public async Task<TossPaymentResponseDto> GetPaymentByOrderIdAsync(string orderId)
    {
        logger.LogInformation("[PaymentService.GetPaymentByOrderIdAsync] OrderId: {OrderId}", orderId);

        var payment = await paymentRepository.GetPaymentByOrderIdAsync(orderId);
        if (payment == null)
        {
            throw new AppException("결제 정보를 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        // Toss API 호출하여 최신 정보 반환
        return await tossPaymentsService.GetPaymentAsync(payment.PaymentKey!);
    }

    // ==================== Private Helper Methods ====================

    /// <summary>
    /// OrderId에서 TransactionId 추출
    /// 형식: TXN_{TransactionId}_{Guid}
    /// </summary>
    private static bool TryExtractTransactionId(string orderId, out long transactionId)
    {
        transactionId = 0;
        var parts = orderId.Split('_');
        if (parts.Length != 3 || parts[0] != "TXN")
        {
            return false;
        }

        return long.TryParse(parts[1], out transactionId);
    }

    /// <summary>
    /// PaymentMethod 조회 또는 생성
    /// </summary>
    private async Task<PaymentMethod> GetOrCreatePaymentMethodAsync(string methodCode)
    {
        // 토스 결제 수단 코드 매핑
        var normalizedCode = MapTossMethodToCode(methodCode);

        var paymentMethod = await paymentRepository.GetPaymentMethodByCodeAsync(normalizedCode);
        if (paymentMethod != null)
        {
            return paymentMethod;
        }

        throw new AppException("결제 수단 코드를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
    }


    /// <summary>
    /// 토스 결제 수단 코드를 DB 코드로 매핑
    /// </summary>
    private static string MapTossMethodToCode(string tossMethod)
    {
        return tossMethod switch
        {
            "카드" => "card",
            "가상계좌" => "virtual_account",
            "계좌이체" => "transfer",
            "휴대폰" => "mobile",
            "간편결제" => "easy_pay",
            _ => "card" // 기본값
        };
    }
}
