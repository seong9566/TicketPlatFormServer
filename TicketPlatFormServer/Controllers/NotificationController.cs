using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Notification;
using TicketPlatFormServer.Services.Notification;

namespace TicketPlatFormServer.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController(INotificationService notificationService) : ControllerBase
{
    [HttpPost("token")]
    public async Task<IActionResult> RegisterToken([FromBody] RegisterTokenReqDto req)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");
        var result = await notificationService.RegisterTokenAsync(userId, req);

        return Ok(new ApiResponse<RegisterTokenRespDto>(
            message: "토큰 등록 성공",
            data: result,
            statusCode: 200
        ));
    }

    [HttpDelete("token")]
    public async Task<IActionResult> DeleteToken([FromBody] DeleteTokenReqDto req)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");
        await notificationService.DeleteTokenAsync(userId, req);

        return Ok(new ApiResponse<object>(
            message: "토큰 삭제 성공",
            data: null,
            statusCode: 200
        ));
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] string? cursor = null, [FromQuery] int? limit = 20)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");
        var result = await notificationService.GetNotificationsAsync(userId, cursor, limit);

        return Ok(new ApiResponse<NotificationListRespDto>(
            message: "알림 목록 조회 성공",
            data: result,
            statusCode: 200
        ));
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> ReadAll()
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");
        var result = await notificationService.MarkAllAsReadAsync(userId);

        return Ok(new ApiResponse<ReadAllRespDto>(
            message: "전체 읽음 처리 완료",
            data: result,
            statusCode: 200
        ));
    }

    [HttpPut("{id:long}/read")]
    public async Task<IActionResult> ReadOne([FromRoute] long id)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");
        await notificationService.MarkAsReadAsync(userId, id);

        return Ok(new ApiResponse<object>(
            message: "읽음 처리 완료",
            data: null,
            statusCode: 200
        ));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount()
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");
        var result = await notificationService.GetUnreadCountAsync(userId);

        return Ok(new ApiResponse<UnreadCountRespDto>(
            message: "미읽음 카운트 조회 성공",
            data: result,
            statusCode: 200
        ));
    }
}
