using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using MySqlConnector;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Chat;
using TicketPlatFormServer.Tests.Helpers;

namespace TicketPlatFormServer.Tests.Tests;

[Collection("Sequential")]
public class ChatFlowTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _buyerClient;
    private readonly HttpClient _sellerClient;

    private int _buyerId;
    private string _buyerEmail = string.Empty;
    private int _sellerId;
    private string _sellerEmail = string.Empty;

    public ChatFlowTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _buyerClient = factory.CreateClient();
        _sellerClient = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await TestDbManager.InitializeAsync();

        await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
        var seeder = new TestDataSeeder(conn);

        var buyer = await CreateUserWithFallbackAsync(seeder, conn);
        _buyerId = buyer.userId;
        _buyerEmail = buyer.email;

        var seller = await CreateUserWithFallbackAsync(seeder, conn);
        _sellerId = seller.userId;
        _sellerEmail = seller.email;

        SetAuthHeader(_buyerClient, _buyerId, _buyerEmail);
        SetAuthHeader(_sellerClient, _sellerId, _sellerEmail);
    }

    public async Task DisposeAsync()
    {
        if (_buyerId > 0 || _sellerId > 0)
        {
            await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
            await conn.OpenAsync();
            var ids = new[] { _buyerId, _sellerId }.Where(id => id > 0).ToArray();
            if (ids.Length > 0)
            {
                var inClause = string.Join(",", ids);
                await using var cmd = new MySqlCommand(
                    $"DELETE FROM user_profile WHERE user_id IN ({inClause}); " +
                    $"DELETE FROM refresh_tokens WHERE user_id IN ({inClause}); " +
                    $"DELETE FROM users WHERE id IN ({inClause});", conn);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    [Fact]
    public async Task ChatFlow_GetChatRooms_Returns200()
    {
        var response = await _buyerClient.GetAsync("/api/chat/rooms");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<List<ChatRoomListRespDto>>>();

        payload.Should().NotBeNull();
        payload!.StatusCode.Should().Be(200);
        payload.Success.Should().BeTrue();
        payload.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ChatFlow_SignalR_LongPolling_CanConnect()
    {
        var token = TestAuthHelper.GenerateUserToken(_buyerId, _buyerEmail);
        var connection = new HubConnectionBuilder()
            .WithUrl($"{_factory.Server.BaseAddress}hubs/chat?access_token={token}", options =>
            {
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        try
        {
            await connection.StartAsync();
            connection.State.Should().Be(HubConnectionState.Connected);
        }
        finally
        {
            await connection.StopAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task ChatFlow_CreateRoom_And_SendMessage()
    {
        var createRoomResponse = await _buyerClient.PostAsJsonAsync("/api/chat/rooms", new CreateChatRoomReqDto
        {
            TicketId = 1
        });

        if (createRoomResponse.StatusCode == HttpStatusCode.OK)
        {
            var createPayload = await createRoomResponse.Content.ReadFromJsonAsync<ApiResponse<ChatRoomDetailRespDto>>();
            createPayload.Should().NotBeNull();
            createPayload!.StatusCode.Should().Be(200);
            createPayload.Success.Should().BeTrue();
            createPayload.Data.Should().NotBeNull();

            var roomId = createPayload.Data!.RoomId;
            roomId.Should().BeGreaterThan(0);

            using var sendForm = new MultipartFormDataContent
            {
                { new StringContent(roomId.ToString()), "RoomId" },
                { new StringContent($"chatflow-message-{Guid.NewGuid():N}"), "Message" }
            };

            var sendResponse = await _buyerClient.PostAsync("/api/chat/messages", sendForm);
            sendResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var sendPayload = await sendResponse.Content.ReadFromJsonAsync<ApiResponse<SendMessageRespDto>>();
            sendPayload.Should().NotBeNull();
            sendPayload!.StatusCode.Should().Be(200);
            sendPayload.Success.Should().BeTrue();
            sendPayload.Data.Should().NotBeNull();
            sendPayload.Data!.RoomId.Should().Be(roomId);

            var getMessagesResponse = await _buyerClient.GetAsync($"/api/chat/messages?roomId={roomId}&limit=50");
            getMessagesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var messagesPayload = await getMessagesResponse.Content.ReadFromJsonAsync<ApiResponse<List<ChatMessageRespDto>>>();
            messagesPayload.Should().NotBeNull();
            messagesPayload!.StatusCode.Should().Be(200);
            messagesPayload.Success.Should().BeTrue();
            messagesPayload.Data.Should().NotBeNull();

            var markReadResponse = await _buyerClient.PostAsJsonAsync("/api/chat/rooms/read", new MarkMessagesAsReadReqDto
            {
                RoomId = roomId
            });
            markReadResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var markReadPayload = await markReadResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();
            markReadPayload.Should().NotBeNull();
            markReadPayload!.StatusCode.Should().Be(200);
            markReadPayload.Success.Should().BeTrue();
            return;
        }

        ((int)createRoomResponse.StatusCode).Should().BeOneOf(400, 404);

        var listResponse = await _buyerClient.GetAsync("/api/chat/rooms");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listPayload = await listResponse.Content.ReadFromJsonAsync<ApiResponse<List<ChatRoomListRespDto>>>();
        listPayload.Should().NotBeNull();
        listPayload!.StatusCode.Should().Be(200);
        listPayload.Success.Should().BeTrue();
    }

    private static void SetAuthHeader(HttpClient client, int userId, string email)
    {
        var token = TestAuthHelper.GenerateUserToken(userId, email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static async Task<(int userId, string email, string password)> CreateUserWithFallbackAsync(
        TestDataSeeder seeder,
        MySqlConnection conn)
    {
        try
        {
            return await seeder.CreateUserAsync(role: "user");
        }
        catch (MySqlException ex) when (ex.Message.Contains("Unknown column 'role'", StringComparison.OrdinalIgnoreCase))
        {
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            var email = $"test_{Guid.NewGuid():N}@test.com";
            const string rawPassword = "Test1234!@#";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(rawPassword);
            var phone = $"010{Random.Shared.Next(10000000, 99999999)}";

            await using var insertUserCmd = new MySqlCommand(@"
                INSERT INTO users (email, password_hash, phone, provider_id, role_id, created_at, is_deleted)
                VALUES (@email, @hash, @phone, @providerId, @roleId, NOW(), 0);", conn);
            insertUserCmd.Parameters.AddWithValue("@email", email);
            insertUserCmd.Parameters.AddWithValue("@hash", hashedPassword);
            insertUserCmd.Parameters.AddWithValue("@phone", phone);
            insertUserCmd.Parameters.AddWithValue("@providerId", 1);
            insertUserCmd.Parameters.AddWithValue("@roleId", 2);
            await insertUserCmd.ExecuteNonQueryAsync();

            var userId = (int)insertUserCmd.LastInsertedId;

            await using var profileCmd = new MySqlCommand(@"
                INSERT INTO user_profile (user_id, nickname)
                VALUES (@userId, @nickname);", conn);
            profileCmd.Parameters.AddWithValue("@userId", userId);
            profileCmd.Parameters.AddWithValue("@nickname", $"TestUser{userId}");
            await profileCmd.ExecuteNonQueryAsync();

            return (userId, email, rawPassword);
        }
    }
}
