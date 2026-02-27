using System.Net;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace TicketPlatFormServer.Tests.Tests;

[Collection("Sequential")]
public class PublicApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PublicApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// GET /api/search - 검색 엔드포인트 (인증 불필요)
    /// </summary>
    [Fact]
    public async Task PublicApi_Search_ReturnsOkWithValidResponse()
    {
        // Arrange
        var keyword = "콘서트";
        var page = 1;
        var pageSize = 10;

        // Act
        var response = await _client.GetAsync($"/api/search?keyword={keyword}&page={page}&pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);

        result.GetProperty("statusCode").GetInt32().Should().Be(200);
        result.GetProperty("success").GetBoolean().Should().BeTrue();
        result.GetProperty("message").GetString().Should().NotBeNullOrWhiteSpace();
        // data can be null or empty list, both are valid
        result.TryGetProperty("data", out _).Should().BeTrue();
    }

    /// <summary>
    /// GET /api/events - 이벤트 목록 조회 (인증 불필요)
    /// </summary>
    [Fact]
    public async Task PublicApi_GetEventsByCategory_ReturnsOkWithValidResponse()
    {
        // Arrange
        var categoryId = 1;

        // Act
        var response = await _client.GetAsync($"/api/events?categoryId={categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);

        result.GetProperty("statusCode").GetInt32().Should().Be(200);
        result.GetProperty("success").GetBoolean().Should().BeTrue();
        result.GetProperty("message").GetString().Should().NotBeNullOrWhiteSpace();
        // data can be null or empty list, both are valid
        result.TryGetProperty("data", out _).Should().BeTrue();
    }

    /// <summary>
    /// GET /api/events/tickets - 이벤트의 티켓 목록 조회 (인증 불필요, optional userId)
    /// </summary>
    [Fact]
    public async Task PublicApi_GetEventDetailWithTickets_ReturnsOkWithValidResponse()
    {
        // Arrange
        var eventId = 1;

        // Act
        var response = await _client.GetAsync($"/api/events/tickets?eventId={eventId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);

        result.GetProperty("statusCode").GetInt32().Should().Be(200);
        result.GetProperty("success").GetBoolean().Should().BeTrue();
        result.GetProperty("message").GetString().Should().NotBeNullOrWhiteSpace();
        // data can be null or empty, both are valid
        result.TryGetProperty("data", out _).Should().BeTrue();
    }

    /// <summary>
    /// GET /api/tickets/detail - 티켓 상세 정보 조회 (인증 불필요, optional userId)
    /// </summary>
    [Fact]
    public async Task PublicApi_GetTicketDetail_ReturnsOkWithValidResponse()
    {
        // Arrange
        var ticketId = 1;

        // Act
        var response = await _client.GetAsync($"/api/tickets/detail?ticketId={ticketId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);

        result.GetProperty("statusCode").GetInt32().Should().Be(200);
        result.GetProperty("success").GetBoolean().Should().BeTrue();
        result.GetProperty("message").GetString().Should().NotBeNullOrWhiteSpace();
        // data can be null or empty, both are valid
        result.TryGetProperty("data", out _).Should().BeTrue();
    }

    /// <summary>
    /// GET /api/home - 홈 화면 데이터 조회 (인증 불필요, optional userId)
    /// </summary>
    [Fact]
    public async Task PublicApi_GetHomeData_ReturnsOkWithValidResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/home");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);

        result.GetProperty("statusCode").GetInt32().Should().Be(200);
        result.GetProperty("success").GetBoolean().Should().BeTrue();
        result.GetProperty("message").GetString().Should().NotBeNullOrWhiteSpace();
        // data can be null or empty, both are valid
        result.TryGetProperty("data", out _).Should().BeTrue();
    }

    /// <summary>
    /// GET /api/home with userId - 홈 화면 데이터 조회 (optional userId 파라미터)
    /// </summary>
    [Fact]
    public async Task PublicApi_GetHomeDataWithUserId_ReturnsOkWithValidResponse()
    {
        // Arrange
        var userId = 1;

        // Act
        var response = await _client.GetAsync($"/api/home?userId={userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);

        result.GetProperty("statusCode").GetInt32().Should().Be(200);
        result.GetProperty("success").GetBoolean().Should().BeTrue();
        result.GetProperty("message").GetString().Should().NotBeNullOrWhiteSpace();
        // data can be null or empty, both are valid
        result.TryGetProperty("data", out _).Should().BeTrue();
    }
}
