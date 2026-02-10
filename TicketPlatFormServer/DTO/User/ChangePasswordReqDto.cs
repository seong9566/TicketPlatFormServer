using System.ComponentModel.DataAnnotations;

namespace TicketPlatFormServer.DTO.User;

/// <summary>
/// 비밀번호 변경 요청 DTO
/// </summary>
public class ChangePasswordReqDto
{
    /// <summary>
    /// 현재 비밀번호
    /// </summary>
    [Required(ErrorMessage = "현재 비밀번호는 필수입니다.")]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// 새 비밀번호 (8자 이상, 영문 대소문자/숫자/특수문자 중 3가지 이상 조합)
    /// </summary>
    [Required(ErrorMessage = "새 비밀번호는 필수입니다.")]
    [MinLength(8, ErrorMessage = "새 비밀번호는 최소 8자 이상이어야 합니다.")]
    public string NewPassword { get; set; } = string.Empty;
}
