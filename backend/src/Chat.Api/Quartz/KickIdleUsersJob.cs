using Chat.Api.Hubs;
using Chat.Application.Services;
using Chat.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using Quartz;

namespace Chat.Api.Quartz;

public class KickIdleUsersJob : IJob
{
    private static readonly TimeSpan IdleTimeout = TimeSpan.FromMinutes(2);

    private readonly UserActivityRegistry _registry;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IChatService _chatService;
    private readonly ILogger<KickIdleUsersJob> _logger;

    public KickIdleUsersJob(
        UserActivityRegistry registry,
        IHubContext<ChatHub> hubContext,
        IChatService chatService,
        ILogger<KickIdleUsersJob> logger)
    {
        _registry = registry;
        _hubContext = hubContext;
        _chatService = chatService;
        _logger = logger;
    }
    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTime.UtcNow;
        var ct = context.CancellationToken;
        var _idleThreshold = IdleTimeout;

        var idleUsers = _registry
            .Snapshot()
            .Where(info => now - info.LastActivityUtc > _idleThreshold)
            .ToList();

        foreach (var userInfo in idleUsers)
        {
            _logger.LogInformation($"Kicking idle user {userInfo.UserId} with connection {userInfo.ConnectionId}");

            var reason = "Kicked for inactivity";

            foreach(var chatRoomId in userInfo.ChatRooms.ToList())
            {
                string? username = null;
                try
                {
                    username = await _chatService.RemoveUserFromChatRoomAsync(chatRoomId, userInfo.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error removing user {userInfo.UserId} from chat room {chatRoomId}");
                }

                await _hubContext.Clients.Group($"chatroom-{chatRoomId}")
                    .SendAsync("UserLeft", new 
                    { userId = userInfo.UserId, 
                      username = username, 
                      message = reason 
                    }, ct);

                await _hubContext.Groups.RemoveFromGroupAsync(userInfo.ConnectionId, $"chatroom-{chatRoomId}");
            }

            await _hubContext.Clients.Client(userInfo.ConnectionId)
                .SendAsync("Kicked", new { message = reason }, ct);

            await _hubContext.Clients.Client(userInfo.ConnectionId)
                .SendAsync("ForceDisconnect", cancellationToken: ct);

            _registry.Remove(userInfo.ConnectionId);
        }
    }
}