using System.ComponentModel.DataAnnotations;

namespace TicketPlatFormServer.DTO.User;

/// <summary>
/// 프로필 수정 요청 DTO
/// </summary>
public class UpdateUserProfileReqDto
{
    /// <summary>
    /// 닉네임 (선택사항)
    /// </summary>
    [MaxLength(50, ErrorMessage = "닉네임은 최대 50자까지 입력 가능합니다.")]
    public string? Nickname { get; set; }

    /// <summary>
    /// 자기소개 (선택사항)
    /// </summary>
    [MaxLength(500, ErrorMessage = "자기소개는 최대 500자까지 입력 가능합니다.")]
    public string? Bio { get; set; }
}
