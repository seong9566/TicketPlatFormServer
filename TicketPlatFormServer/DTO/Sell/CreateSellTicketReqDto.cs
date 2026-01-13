using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TicketPlatFormServer.DTO.Sell;

/// <summary>
/// 티켓 판매 등록 요청 DTO
/// </summary>
public class CreateSellTicketReqDto
{
    /// <summary>
    /// 공연 ID (필수)
    /// </summary>
    [Required(ErrorMessage = "공연 ID는 필수입니다.")]
    public int EventId { get; set; }

    /// <summary>
    /// 일정 ID (필수)
    /// </summary>
    [Required(ErrorMessage = "일정 ID는 필수입니다.")]
    public string ScheduleId { get; set; } = null!;

    /// <summary>
    /// 좌석 위치 ID (선택)
    /// </summary>
    public string? LocationId { get; set; }

    /// <summary>
    /// 구역 (예: A구역)
    /// </summary>
    public string? Area { get; set; }

    /// <summary>
    /// 열 (예: 5열)
    /// </summary>
    public string? Row { get; set; }

    /// <summary>
    /// 좌석 정보 (필수)
    /// </summary>
    [Required(ErrorMessage = "좌석 정보는 필수입니다.")]
    public string SeatInfo { get; set; } = null!;

    /// <summary>
    /// 연석 여부
    /// </summary>
    public bool IsConsecutive { get; set; } = false;

    /// <summary>
    /// 수량 (필수)
    /// </summary>
    [Required(ErrorMessage = "수량은 필수입니다.")]
    [Range(1, 100, ErrorMessage = "수량은 1~100 사이여야 합니다.")]
    public int Quantity { get; set; }

    /// <summary>
    /// 판매가 (필수)
    /// </summary>
    [Required(ErrorMessage = "판매가는 필수입니다.")]
    [Range(1, int.MaxValue, ErrorMessage = "판매가는 1원 이상이어야 합니다.")]
    public int Price { get; set; }

    /// <summary>
    /// 정가 (필수)
    /// </summary>
    [Required(ErrorMessage = "정가는 필수입니다.")]
    [Range(1, int.MaxValue, ErrorMessage = "정가는 1원 이상이어야 합니다.")]
    public int OriginalPrice { get; set; }

    /// <summary>
    /// 상세 설명 (선택)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 티켓 이미지 파일들 (최대 5개)
    /// </summary>
    public List<IFormFile>? Images { get; set; }
}
