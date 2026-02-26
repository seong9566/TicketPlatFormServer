namespace TicketPlatFormServer.Services.Email;

public interface IEmailService
{
    /// <summary>
    /// 임시 비밀번호 안내 이메일을 발송합니다.
    /// </summary>
    /// <param name="toEmail">수신자 이메일 주소</param>
    /// <param name="tempPassword">임시 비밀번호</param>
    Task SendTemporaryPasswordEmailAsync(string toEmail, string tempPassword);
}
