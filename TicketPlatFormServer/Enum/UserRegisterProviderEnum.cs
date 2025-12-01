namespace TicketPlatFormServer.Enum;

/// <summary>
/// 회원가입 유형
/// Email : 기본
/// OAuth : Google,KaKao,Apple
/// </summary>
public enum UserRegisterProviderEnum
{
    Email,
    Google,
    KaKao,
    Apple
}