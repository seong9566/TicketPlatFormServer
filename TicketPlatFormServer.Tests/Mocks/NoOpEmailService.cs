using TicketPlatFormServer.Services.Email;

namespace TicketPlatFormServer.Tests.Mocks;

/// <summary>
/// SMTP 이메일 서비스 No-Op 구현 (테스트용 — 실제 이메일 발송 없음)
/// </summary>
public class NoOpEmailService : IEmailService
{
    public Task SendTemporaryPasswordEmailAsync(string toEmail, string tempPassword)
        => Task.CompletedTask;
}
