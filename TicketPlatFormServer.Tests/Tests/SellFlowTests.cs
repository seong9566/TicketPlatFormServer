using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Sell;
using TicketPlatFormServer.Tests.Helpers;

namespace TicketPlatFormServer.Tests.Tests;

[Collection("Sequential")]
public class SellFlowTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SellFlowTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        TestAuthHelper.AddAuthHeader(_client, userId: 1, email: "seller@test.com");
    }

    [Fact]
    public async Task SellFlow_GetCategories_Returns200_WithApiEnvelope()
    {
        var response = await _client.GetAsync("/api/sell/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<List<CategoryRespDto>>>();
        payload.Should().NotBeNull();
        payload!.StatusCode.Should().Be(200);
        payload.Success.Should().BeTrue();
        payload.Message.Should().NotBeNullOrWhiteSpace();
        payload.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task SellFlow_GetFeatures_Returns200_WithApiEnvelope()
    {
        var response = await _client.GetAsync("/api/sell/features");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<List<TicketFeatureRespDto>>>();
        payload.Should().NotBeNull();
        payload!.StatusCode.Should().Be(200);
        payload.Success.Should().BeTrue();
        payload.Message.Should().NotBeNullOrWhiteSpace();
        payload.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task SellFlow_GetTradeMethods_Returns200_WithApiEnvelope()
    {
        var response = await _client.GetAsync("/api/sell/trade-methods");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<List<TradeMethodRespDto>>>();
        payload.Should().NotBeNull();
        payload!.StatusCode.Should().Be(200);
        payload.Success.Should().BeTrue();
        payload.Message.Should().NotBeNullOrWhiteSpace();
        payload.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task SellFlow_WizardStep4_EndToEnd()
    {
        var categoriesResponse = await _client.GetAsync("/api/sell/categories");
        categoriesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var categories = await categoriesResponse.Content.ReadFromJsonAsync<ApiResponse<List<CategoryRespDto>>>();
        categories.Should().NotBeNull();
        categories!.StatusCode.Should().Be(200);
        categories.Success.Should().BeTrue();
        categories.Data.Should().NotBeNull();
        categories.Data.Should().NotBeEmpty("categories should be seeded for sell flow");

        var categoryId = categories.Data!.First().CategoryId;

        var eventsResponse = await _client.GetAsync($"/api/sell/events?categoryId={categoryId}&page=1&size=20");
        eventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var eventsPayload = await eventsResponse.Content.ReadFromJsonAsync<ApiResponse<SellEventListRespDto>>();
        eventsPayload.Should().NotBeNull();
        eventsPayload!.StatusCode.Should().Be(200);
        eventsPayload.Success.Should().BeTrue();
        eventsPayload.Data.Should().NotBeNull();

        var eventItem = eventsPayload.Data!.Events.FirstOrDefault();
        if (eventItem is null)
        {
            var myTicketsResponseWithoutEvent = await _client.GetAsync("/api/sell/my-tickets?page=1&size=20");
            myTicketsResponseWithoutEvent.StatusCode.Should().Be(HttpStatusCode.OK);

            var myTicketsPayloadWithoutEvent =
                await myTicketsResponseWithoutEvent.Content.ReadFromJsonAsync<ApiResponse<MyTicketListRespDto>>();
            myTicketsPayloadWithoutEvent.Should().NotBeNull();
            myTicketsPayloadWithoutEvent!.StatusCode.Should().Be(200);
            myTicketsPayloadWithoutEvent.Success.Should().BeTrue();
            return;
        }

        var eventId = eventItem.EventId;

        var schedulesResponse = await _client.GetAsync($"/api/sell/events/schedules?eventId={eventId}");
        schedulesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var schedulesPayload = await schedulesResponse.Content.ReadFromJsonAsync<ApiResponse<EventScheduleRespDto>>();
        schedulesPayload.Should().NotBeNull();
        schedulesPayload!.StatusCode.Should().Be(200);
        schedulesPayload.Success.Should().BeTrue();
        schedulesPayload.Data.Should().NotBeNull();

        var seatOptionsResponse = await _client.GetAsync($"/api/sell/events/seat-options?eventId={eventId}");
        seatOptionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var seatOptionsPayload = await seatOptionsResponse.Content.ReadFromJsonAsync<ApiResponse<SeatOptionRespDto>>();
        seatOptionsPayload.Should().NotBeNull();
        seatOptionsPayload!.StatusCode.Should().Be(200);
        seatOptionsPayload.Success.Should().BeTrue();
        seatOptionsPayload.Data.Should().NotBeNull();

        var tradeMethodsResponse = await _client.GetAsync("/api/sell/trade-methods");
        tradeMethodsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var tradeMethodsPayload =
            await tradeMethodsResponse.Content.ReadFromJsonAsync<ApiResponse<List<TradeMethodRespDto>>>();
        tradeMethodsPayload.Should().NotBeNull();
        tradeMethodsPayload!.StatusCode.Should().Be(200);
        tradeMethodsPayload.Success.Should().BeTrue();
        tradeMethodsPayload.Data.Should().NotBeNull();

        var scheduleId = schedulesPayload.Data!.Schedules.FirstOrDefault()?.ScheduleId;
        var seatGradeId = seatOptionsPayload.Data!.Grades.FirstOrDefault()?.GradeId;
        var locationId = seatOptionsPayload.Data.Locations.FirstOrDefault()?.LocationId;
        var areaId = seatOptionsPayload.Data.Areas.FirstOrDefault()?.AreaId;
        var tradeMethodId = tradeMethodsPayload.Data!.FirstOrDefault()?.Id;

        int originalPrice = 50000;
        if (seatGradeId.HasValue)
        {
            var originalPriceQuery = $"/api/sell/events/original-price?eventId={eventId}&gradeId={seatGradeId.Value}";
            if (locationId.HasValue)
            {
                originalPriceQuery += $"&locationId={locationId.Value}";
            }

            if (areaId.HasValue)
            {
                originalPriceQuery += $"&areaId={areaId.Value}";
            }

            var originalPriceResponse = await _client.GetAsync(originalPriceQuery);
            originalPriceResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var originalPricePayload = await originalPriceResponse.Content.ReadFromJsonAsync<ApiResponse<int?>>();
            originalPricePayload.Should().NotBeNull();
            originalPricePayload!.StatusCode.Should().Be(200);
            originalPricePayload.Success.Should().BeTrue();

            if (originalPricePayload.Data is > 0)
            {
                originalPrice = originalPricePayload.Data.Value;
            }
        }

        int? createdTicketId = null;
        if (!string.IsNullOrWhiteSpace(scheduleId) && seatGradeId.HasValue && tradeMethodId.HasValue)
        {
            using var formContent = new MultipartFormDataContent
            {
                { new StringContent(eventId.ToString()), "EventId" },
                { new StringContent(scheduleId), "ScheduleId" },
                { new StringContent(seatGradeId.Value.ToString()), "SeatGradeId" },
                { new StringContent(tradeMethodId.Value.ToString()), "TradeMethodId" },
                { new StringContent("true"), "HasTicket" },
                { new StringContent("1"), "Quantity" },
                { new StringContent(originalPrice.ToString()), "Price" },
                { new StringContent(originalPrice.ToString()), "OriginalPrice" },
                { new StringContent("E2E sell flow ticket"), "Description" },
                { new StringContent("A-1"), "Row" }
            };

            if (locationId.HasValue)
            {
                formContent.Add(new StringContent(locationId.Value.ToString()), "LocationId");
            }

            if (areaId.HasValue)
            {
                formContent.Add(new StringContent(areaId.Value.ToString()), "AreaId");
            }

            var createResponse = await _client.PostAsync("/api/sell/tickets", formContent);

            if (createResponse.StatusCode == HttpStatusCode.OK)
            {
                var createPayload =
                    await createResponse.Content.ReadFromJsonAsync<ApiResponse<CreateSellTicketRespDto>>();
                createPayload.Should().NotBeNull();
                createPayload!.StatusCode.Should().Be(200);
                createPayload.Success.Should().BeTrue();
                createPayload.Data.Should().NotBeNull();
                createPayload.Data!.TicketId.Should().BeGreaterThan(0);
                createdTicketId = createPayload.Data.TicketId;
            }
            else
            {
                ((int)createResponse.StatusCode).Should().BeOneOf(400, 404);
                var errorPayload = await createResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();
                errorPayload.Should().NotBeNull();
                errorPayload!.StatusCode.Should().Be((int)createResponse.StatusCode);
                errorPayload.Message.Should().NotBeNullOrWhiteSpace();
            }
        }

        var myTicketsResponse = await _client.GetAsync("/api/sell/my-tickets?page=1&size=20");
        myTicketsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var myTicketsPayload = await myTicketsResponse.Content.ReadFromJsonAsync<ApiResponse<MyTicketListRespDto>>();
        myTicketsPayload.Should().NotBeNull();
        myTicketsPayload!.StatusCode.Should().Be(200);
        myTicketsPayload.Success.Should().BeTrue();
        myTicketsPayload.Data.Should().NotBeNull();

        if (createdTicketId.HasValue)
        {
            var cancelResponse = await _client.DeleteAsync($"/api/sell/tickets?ticketId={createdTicketId.Value}");
            cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var cancelPayload = await cancelResponse.Content.ReadFromJsonAsync<ApiResponse<CancelSellTicketRespDto>>();
            cancelPayload.Should().NotBeNull();
            cancelPayload!.StatusCode.Should().Be(200);
            cancelPayload.Success.Should().BeTrue();
            cancelPayload.Data.Should().NotBeNull();
            cancelPayload.Data!.TicketId.Should().Be(createdTicketId.Value);
        }
    }
}
