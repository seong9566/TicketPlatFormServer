namespace TicketPlatFormServer.Enum;

/// <summary>
/// 회원가입 유형
/// Email : 기본
/// OAuth : Google,KaKao,Apple
/// </summary>
public enum UserRegisterProviderEnum
// 닷넷 Enum 클래스 Value 출력 문법
// Enum.GetValues(typeof(<Value>))
{
    Email,
    Google,
    KaKao,
    Apple
}