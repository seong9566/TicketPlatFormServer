using System.Net;
using Microsoft.Extensions.Options;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.Config;

#pragma warning disable SYSLIB0027

namespace TicketPlatFormServer.Services.Email;

public class SmtpEmailService(
    IOptions<EmailSettings> emailOptions,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly EmailSettings _settings = emailOptions.Value;

    public async Task SendTemporaryPasswordEmailAsync(string toEmail, string tempPassword)
    {
        try
        {
            using var client = new System.Net.Mail.SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPassword),
                EnableSsl = true
            };

            using var message = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = "[TicketHub] 임시 비밀번호 안내",
                Body = BuildEmailBody(tempPassword),
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);

            logger.LogInformation("[Email] 임시 비밀번호 이메일 발송 완료: {ToEmail}", toEmail);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            logger.LogError(ex, "[Email] 이메일 발송 실패: {ToEmail}", toEmail);
            throw new AppException("이메일 발송에 실패했습니다.", HttpStatusCode.InternalServerError, ex);
        }
    }

    private static string BuildEmailBody(string tempPassword)
    {
        return $"""
            <html>
            <body style="font-family: Arial, sans-serif; line-height: 1.6;">
              <h2 style="color: #333;">[TicketHub] 임시 비밀번호 안내</h2>
              <p>안녕하세요. TicketHub 고객센터입니다.</p>
              <p>요청하신 임시 비밀번호가 발급되었습니다.</p>
              <table style="border: 1px solid #ddd; padding: 16px; border-radius: 4px;">
                <tr>
                  <td><strong>임시 비밀번호</strong></td>
                  <td style="padding-left: 16px; font-size: 18px; letter-spacing: 2px;">{tempPassword}</td>
                </tr>
              </table>
              <p style="color: #e53e3e; margin-top: 16px;">
                ⚠️ 보안을 위해 로그인 후 반드시 비밀번호를 변경해 주세요.
              </p>
              <p>감사합니다.<br/>TicketHub 팀</p>
            </body>
            </html>
            """;
    }
}

#pragma warning restore SYSLIB0027
