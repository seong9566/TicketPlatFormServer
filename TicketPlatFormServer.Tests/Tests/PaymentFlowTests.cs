using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Payment;
using TicketPlatFormServer.Tests.Helpers;

namespace TicketPlatFormServer.Tests.Tests;

[Collection("Sequential")]
public class PaymentFlowTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PaymentFlowTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        TestAuthHelper.AddAuthHeader(_client, userId: 1, email: "buyer@test.com");
    }

    [Fact]
    public async Task PaymentFlow_Webhook_Simulation_Returns200()
    {
        var webhook = new TossWebhookDto
        {
            EventType = "PAYMENT_STATUS_CHANGED",
            CreatedAt = DateTimeOffset.UtcNow.ToString("O"),
            Data = new TossPaymentResponseDto
            {
                PaymentKey = $"test_paymentKey_{Guid.NewGuid():N}",
                OrderId = $"test_order_{Guid.NewGuid():N}",
                OrderName = "Webhook Test Payment",
                Status = "DONE",
                RequestedAt = DateTimeOffset.UtcNow.ToString("O"),
                Method = "카드",
                TotalAmount = 1000,
                BalanceAmount = 1000,
                SuppliedAmount = 909,
                Vat = 91,
                TaxFreeAmount = 0
            }
        };

        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync("/api/payment/webhook", webhook);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<TossWebhookResponse>();
        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task PaymentFlow_GetPaymentByOrderId_NotFound_Returns4xx()
    {
        var orderId = $"NONEXISTENT_{Guid.NewGuid():N}";

        // No auth needed for this test — clear any existing auth header
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync($"/api/payment/order/{orderId}");

        ((int)response.StatusCode).Should().BeInRange(400, 499);
    }

    [Fact]
    public async Task PaymentFlow_RequestPayment_WithoutTransaction_Returns4xx()
    {
        var request = new PaymentRequestDto
        {
            TransactionId = long.MaxValue,
            Amount = 1000,
            OrderName = "E2E test order",
            CustomerName = "Test Buyer",
            CustomerEmail = "buyer@test.com"
        };

        // Auth required for payment request
        var response = await _client.PostAsJsonAsync("/api/payment/request", request);

        ((int)response.StatusCode).Should().BeInRange(400, 499);
    }
}
