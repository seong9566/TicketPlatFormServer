using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.Repository.Chat;

namespace TicketPlatFormServer.Hubs;

/// <summary>
/// 실시간 채팅을 위한 SignalR Hub
/// </summary>
[Authorize]
public class ChatHub(IChatRepository chatRepo, ILogger<ChatHub> logger) : Hub
{
    /// <summary>
    /// 클라이언트 연결 시
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.GetUserId();
        if (!userId.HasValue)
        {
            var claimTypes = Context.User?.Claims.Select(c => c.Type).Distinct().ToList() ?? new List<string>();
            logger.LogWarning("[ChatHub.OnConnectedAsync] Missing userId claim. ConnectionId={ConnectionId}, Claims={Claims}",
                Context.ConnectionId, string.Join(",", claimTypes));
        }
        if (userId.HasValue)
        {
            // 사용자별 그룹에 추가
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            logger.LogInformation("[ChatHub.OnConnectedAsync] User {UserId} connected with ConnectionId: {ConnectionId}",
                userId, Context.ConnectionId);
            logger.LogInformation("[ChatHub.OnConnectedAsync] User {UserId} joined group user_{GroupUserId}. ConnectionId={ConnectionId}",
                userId, userId, Context.ConnectionId);
        }
        else
        {
            logger.LogWarning("[ChatHub.OnConnectedAsync] Connection without valid userId: {ConnectionId}",
                Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// 클라이언트 연결 해제 시
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.GetUserId();
        if (userId.HasValue)
        {
            logger.LogInformation("[ChatHub.OnDisconnectedAsync] User {UserId} disconnected: {ConnectionId}",
                userId, Context.ConnectionId);
        }

        if (exception != null)
        {
            logger.LogError(exception, "[ChatHub.OnDisconnectedAsync] Connection error for {ConnectionId}",
                Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// 특정 채팅방에 참여
    /// </summary>
    public async Task JoinRoom(long roomId)
    {
        var userId = Context.User?.GetUserId();
        if (!userId.HasValue)
        {
            throw new HubException("인증되지 않은 사용자입니다.");
        }

        // 권한 확인 - 사용자가 해당 채팅방에 속해 있는지 검증
        var isInRoom = await chatRepo.IsUserInChatRoom(roomId, userId.Value);
        if (!isInRoom)
        {
            throw new HubException("이 채팅방에 접근할 권한이 없습니다.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"room_{roomId}");
        logger.LogInformation("[ChatHub.JoinRoom] User {UserId} joined room {RoomId}", userId, roomId);

        // 채팅방의 다른 사용자들에게 알림
        await Clients.OthersInGroup($"room_{roomId}").SendAsync("UserJoined", new
        {
            UserId = userId.Value,
            RoomId = roomId,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// 채팅방에서 나가기
    /// </summary>
    public async Task LeaveRoom(long roomId)
    {
        var userId = Context.User?.GetUserId();
        if (!userId.HasValue)
        {
            throw new HubException("인증되지 않은 사용자입니다.");
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room_{roomId}");
        logger.LogInformation("[ChatHub.LeaveRoom] User {UserId} left room {RoomId}", userId, roomId);

        // 채팅방의 다른 사용자들에게 알림
        await Clients.OthersInGroup($"room_{roomId}").SendAsync("UserLeft", new
        {
            UserId = userId.Value,
            RoomId = roomId,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// 타이핑 중 알림
    /// </summary>
    public async Task UserTyping(long roomId)
    {
        var userId = Context.User?.GetUserId();
        if (!userId.HasValue)
        {
            throw new HubException("인증되지 않은 사용자입니다.");
        }

        // 권한 확인
        var isInRoom = await chatRepo.IsUserInChatRoom(roomId, userId.Value);
        if (!isInRoom)
        {
            throw new HubException("이 채팅방에 접근할 권한이 없습니다.");
        }

        // 같은 방의 다른 사용자들에게만 타이핑 알림
        await Clients.OthersInGroup($"room_{roomId}").SendAsync("UserTyping", new
        {
            UserId = userId.Value,
            RoomId = roomId,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// 타이핑 중지 알림
    /// </summary>
    public async Task UserStoppedTyping(long roomId)
    {
        var userId = Context.User?.GetUserId();
        if (!userId.HasValue)
        {
            throw new HubException("인증되지 않은 사용자입니다.");
        }

        // 권한 확인
        var isInRoom = await chatRepo.IsUserInChatRoom(roomId, userId.Value);
        if (!isInRoom)
        {
            throw new HubException("이 채팅방에 접근할 권한이 없습니다.");
        }

        // 같은 방의 다른 사용자들에게만 알림
        await Clients.OthersInGroup($"room_{roomId}").SendAsync("UserStoppedTyping", new
        {
            UserId = userId.Value,
            RoomId = roomId,
            Timestamp = DateTime.UtcNow
        });
    }
}
