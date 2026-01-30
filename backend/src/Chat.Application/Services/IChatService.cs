using Chat.Application.DTOs;

namespace Chat.Application.Services;

public interface IChatService
{
    Task<ChatRoomDto> CreateChatRoomAsync(CreateChatRoomRequest request, Guid createdBy);
    Task<ChatRoomDto> GetChatRoomAsync(Guid chatRoomId);
    Task<List<ChatRoomDto>> GetUserChatRoomsAsync(Guid userId);
    Task<MessageDto> SendMessageAsync(Guid chatRoomId, Guid senderId, string text);
    Task<List<MessageDto>> GetChatMessagesAsync(Guid chatRoomId, int limit = 50);
    Task<bool> DeleteChatRoomAsync(Guid chatRoomId, Guid userId);
    Task<bool> AddUserToChatRoomAsync(Guid chatRoomId, string username, Guid invitedBy);
    Task<string?> RemoveUserFromChatRoomAsync(Guid chatRoomId, Guid userId);
}
