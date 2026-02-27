using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using MySqlConnector;
using TicketPlatFormServer.DTO;
using Xunit;

namespace TicketPlatFormServer.Tests.Tests;

[CollectionDefinition("Sequential", DisableParallelization = true)]
public class SequentialCollection;

[Collection("Sequential")]
public class AuthFlowTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthFlowTests()
    {
        _factory = new AuthFlowWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await TestDbManager.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        try
        {
            await TestDbManager.CleanupAsync();
        }
        catch (MySqlException ex) when (ex.Message.Contains("profile_image", StringComparison.OrdinalIgnoreCase))
        {
            await CleanupAuthUsersAsync();
        }

        await _factory.DisposeAsync();
    }

    private static async Task CleanupAuthUsersAsync()
    {
        await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
        await conn.OpenAsync();

        var commands = new[]
        {
            "DELETE rt FROM refresh_tokens rt INNER JOIN users u ON rt.user_id = u.id WHERE u.email LIKE 'test_%@test.com'",
            "DELETE up FROM user_profile up INNER JOIN users u ON up.user_id = u.id WHERE u.email LIKE 'test_%@test.com'",
            "DELETE FROM users WHERE email LIKE 'test_%@test.com'"
        };

        foreach (var sql in commands)
        {
            await using var deleteCmd = new MySqlCommand(sql, conn);
            await deleteCmd.ExecuteNonQueryAsync();
        }
    }

    [Fact]
    public async Task AuthFlow_Register_Login_Refresh_Logout()
    {
        var email = $"test_{Guid.NewGuid():N}@test.com";
        const string password = "Test1234!@";

        var signReq = new RegisterUserReqDto
        {
            Email = email,
            Password = password,
            Provider = "email",
            Role = "user"
        };
        var signResp = await _client.PostAsJsonAsync("/api/auth/sign", signReq);
        signResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var signResult = await signResp.Content.ReadFromJsonAsync<ApiResponse<RegisterUserRespDto>>();
        signResult.Should().NotBeNull();
        signResult!.Success.Should().BeTrue();
        signResult.StatusCode.Should().Be(200);
        signResult.Data.Should().NotBeNull();
        signResult.Data!.Email.Should().Be(email);

        var loginReq = new LoginUserReqDto
        {
            Email = email,
            Password = password
        };
        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", loginReq);
        loginResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginResult = await loginResp.Content.ReadFromJsonAsync<ApiResponse<LoginUserRespDto>>();
        loginResult.Should().NotBeNull();
        loginResult!.Success.Should().BeTrue();
        loginResult.StatusCode.Should().Be(200);
        loginResult.Data.Should().NotBeNull();
        loginResult.Data!.Email.Should().Be(email);
        loginResult.Data.AccessToken.Should().NotBeNullOrWhiteSpace();
        loginResult.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();

        var refreshReq = new RefreshTokenReqDto
        {
            RefreshToken = loginResult.Data.RefreshToken!
        };
        var refreshResp = await _client.PostAsJsonAsync("/api/auth/refresh", refreshReq);
        refreshResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshResult = await refreshResp.Content.ReadFromJsonAsync<ApiResponse<TokenResponseDto>>();
        refreshResult.Should().NotBeNull();
        refreshResult!.Success.Should().BeTrue();
        refreshResult.StatusCode.Should().Be(200);
        refreshResult.Data.Should().NotBeNull();
        refreshResult.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        refreshResult.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", refreshResult.Data.AccessToken);

        var logoutReq = new RefreshTokenReqDto
        {
            RefreshToken = refreshResult.Data.RefreshToken
        };
        var logoutResp = await _client.PostAsJsonAsync("/api/auth/logout", logoutReq);
        logoutResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var logoutResult = await logoutResp.Content.ReadFromJsonAsync<ApiResponse<object>>();
        logoutResult.Should().NotBeNull();
        logoutResult!.Success.Should().BeTrue();
        logoutResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task AuthFlow_InvalidPassword_Returns4xx()
    {
        var email = $"test_{Guid.NewGuid():N}@test.com";

        var signResp = await _client.PostAsJsonAsync("/api/auth/sign", new RegisterUserReqDto
        {
            Email = email,
            Password = "Correct1234!@",
            Provider = "email",
            Role = "user"
        });
        signResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var signResult = await signResp.Content.ReadFromJsonAsync<ApiResponse<RegisterUserRespDto>>();
        signResult.Should().NotBeNull();
        signResult!.Success.Should().BeTrue();

        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", new LoginUserReqDto
        {
            Email = email,
            Password = "WrongPassword999!"
        });

        ((int)loginResp.StatusCode).Should().BeInRange(400, 499);
        var loginResult = await loginResp.Content.ReadFromJsonAsync<ApiResponse<object>>();
        loginResult.Should().NotBeNull();
        loginResult!.Success.Should().BeFalse();
        loginResult.StatusCode.Should().Be((int)loginResp.StatusCode);
        loginResult.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task AuthFlow_DuplicateEmail_Returns4xx()
    {
        var email = $"test_{Guid.NewGuid():N}@test.com";
        const string password = "Test1234!@";

        var firstResp = await _client.PostAsJsonAsync("/api/auth/sign", new RegisterUserReqDto
        {
            Email = email,
            Password = password,
            Provider = "email",
            Role = "user"
        });
        firstResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstResult = await firstResp.Content.ReadFromJsonAsync<ApiResponse<RegisterUserRespDto>>();
        firstResult.Should().NotBeNull();
        firstResult!.Success.Should().BeTrue();

        var duplicateResp = await _client.PostAsJsonAsync("/api/auth/sign", new RegisterUserReqDto
        {
            Email = email,
            Password = password,
            Provider = "email",
            Role = "user"
        });

        var duplicateStatus = (int)duplicateResp.StatusCode;
        (duplicateStatus == 208 || (duplicateStatus >= 400 && duplicateStatus <= 499)).Should().BeTrue();
        var duplicateResult = await duplicateResp.Content.ReadFromJsonAsync<ApiResponse<object>>();
        duplicateResult.Should().NotBeNull();
        duplicateResult!.StatusCode.Should().Be(duplicateStatus);
        duplicateResult.Message.Should().NotBeNullOrWhiteSpace();
        duplicateResult.Message.Should().Contain("가입");
    }

    private sealed class AuthFlowWebApplicationFactory : TestWebApplicationFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseEnvironment("Development");
        }
    }
}
