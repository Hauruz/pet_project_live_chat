using Chat.Application.DTOs;
using Chat.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Chat.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        _logger.LogInformation($"User {username} ({userId}) connected. ConnectionId: {Context.ConnectionId}");
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        _logger.LogInformation($"User {username} ({userId}) disconnected. ConnectionId: {Context.ConnectionId}");
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinChatRoom(Guid chatRoomId)
    {
        try
        {
            var userId = GetUserId();
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chatroom-{chatRoomId}");
            
            _logger.LogInformation($"User {userId} joined chat room {chatRoomId}");
            
            await Clients.Group($"chatroom-{chatRoomId}")
                .SendAsync("UserJoined", new { userId = userId, message = "User joined the chat" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining chat room");
            await Clients.Caller.SendAsync("Error", new { message = "Failed to join chat room" });
        }
    }

    public async Task LeaveChatRoom(Guid chatRoomId)
    {
        try
        {
            var userId = GetUserId();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chatroom-{chatRoomId}");
            
            _logger.LogInformation($"User {userId} left chat room {chatRoomId}");
            
            await Clients.Group($"chatroom-{chatRoomId}")
                .SendAsync("UserLeft", new { userId = userId, message = "User left the chat" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving chat room");
            await Clients.Caller.SendAsync("Error", new { message = "Failed to leave chat room" });
        }
    }

    public async Task SendMessage(Guid chatRoomId, string text)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                await Clients.Caller.SendAsync("Error", new { message = "Message cannot be empty" });
                return;
            }

            var userId = GetUserId();
            var messageDto = await _chatService.SendMessageAsync(chatRoomId, userId, text);

            _logger.LogInformation($"Message sent in chat room {chatRoomId} by user {userId}");

            // Send message to all clients in the chat room
            await Clients.Group($"chatroom-{chatRoomId}")
                .SendAsync("ReceiveMessage", messageDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            await Clients.Caller.SendAsync("Error", new { message = "Failed to send message" });
        }
    }

    public async Task<List<MessageDto>> GetChatHistory(Guid chatRoomId, int limit = 50)
    {
        try
        {
            return await _chatService.GetChatMessagesAsync(chatRoomId, limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat history");
            await Clients.Caller.SendAsync("Error", new { message = "Failed to load chat history" });
            return new List<MessageDto>();
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;

        throw new InvalidOperationException("Unable to get user ID from claims");
    }
}
