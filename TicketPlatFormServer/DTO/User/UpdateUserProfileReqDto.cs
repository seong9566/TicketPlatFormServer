using System.ComponentModel.DataAnnotations;

namespace TicketPlatFormServer.DTO.User;

/// <summary>
/// 프로필 수정 요청 DTO
/// </summary>
public class UpdateUserProfileReqDto
{
    /// <summary>
    /// 닉네임 (선택사항)
    /// Flutter 팀 요구사항: 2~20자, 한글/영문/숫자/언더스코어/하이픈만 허용
    /// </summary>
    [MinLength(2, ErrorMessage = "닉네임은 최소 2자 이상이어야 합니다.")]
    [MaxLength(20, ErrorMessage = "닉네임은 최대 20자까지 입력 가능합니다.")]
    [RegularExpression(@"^[가-힣a-zA-Z0-9_-]+$",
        ErrorMessage = "닉네임에 허용되지 않는 문자가 포함되어 있습니다.")]
    public string? Nickname { get; set; }

    /// <summary>
    /// 자기소개 (선택사항)
    /// Flutter 팀 요구사항: 최대 200자
    /// </summary>
    [MaxLength(200, ErrorMessage = "자기소개는 최대 200자까지 입력 가능합니다.")]
    public string? Bio { get; set; }

    /// <summary>
    /// 프로필 이미지 파일 (선택사항)
    /// </summary>
    public IFormFile? ProfileImage { get; set; }

    /// <summary>
    /// 프로필 이미지 삭제 플래그 (기본값: false)
    /// </summary>
    public bool RemoveProfileImage { get; set; } = false;
}
