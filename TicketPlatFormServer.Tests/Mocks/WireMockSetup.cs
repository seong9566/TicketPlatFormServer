using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace TicketPlatFormServer.Tests.Mocks;

/// <summary>
/// WireMock 서버 — 외부 서비스(Supabase, OAuth, FCM) Mock
/// TossPayments는 실제 테스트 API 사용 (Mock 안 함)
/// </summary>
public class WireMockSetup : IDisposable
{
    public WireMockServer Server { get; }
    public string Url => Server.Url!;

    public WireMockSetup()
    {
        Server = WireMockServer.Start();
        RegisterStubs();
    }

    private void RegisterStubs()
    {
        // Supabase Storage upload stub (any path, POST)
        Server
            .Given(Request.Create().WithPath("/storage/v1/object/*").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"Key\": \"test/uploaded-file.jpg\"}"));

        // Supabase Storage signed URL stub (POST)
        Server
            .Given(Request.Create().WithPath("/storage/v1/object/sign/*").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"signedURL\": \"https://test.supabase.co/storage/v1/object/sign/test/file.jpg?token=test\"}"));

        // Google OAuth token exchange stub
        Server
            .Given(Request.Create().WithPath("/oauth2/v4/token").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"access_token\": \"mock_google_access_token\", \"token_type\": \"Bearer\"}"));

        // Google user info stub
        Server
            .Given(Request.Create().WithPath("/oauth2/v2/userinfo").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"id\": \"12345\", \"email\": \"test@gmail.com\", \"name\": \"Test User\"}"));

        // Kakao OAuth token exchange stub
        Server
            .Given(Request.Create().WithPath("/oauth/token").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"access_token\": \"mock_kakao_access_token\", \"token_type\": \"bearer\"}"));

        // Kakao user info stub
        Server
            .Given(Request.Create().WithPath("/v2/user/me").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"id\": 99999, \"kakao_account\": {\"email\": \"test@kakao.com\", \"profile\": {\"nickname\": \"TestUser\"}}}"));

        // FCM send notification stub
        Server
            .Given(Request.Create().WithPath("/v1/projects/*/messages:send").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"name\": \"projects/test/messages/12345\"}"));
    }

    public void Dispose()
    {
        Server.Stop();
        Server.Dispose();
    }
}
