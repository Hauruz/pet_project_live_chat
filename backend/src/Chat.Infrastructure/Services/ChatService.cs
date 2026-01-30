using Chat.Application.DTOs;
using Chat.Application.Services;
using Chat.Domain.Entities;
using Chat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Chat.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly ChatDbContext _dbContext;

    public ChatService(ChatDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ChatRoomDto> CreateChatRoomAsync(CreateChatRoomRequest request, Guid createdBy)
    {
        var chatRoom = new ChatRoom
        {
            Id = Guid.NewGuid(),
            Type = request.Type,
            Title = request.Title,
            CreatedByUserId = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        var members = new List<ChatMember>
        {
            new ChatMember { ChatRoomId = chatRoom.Id, UserId = createdBy }
        };

        foreach (var memberId in request.MemberIds)
        {
            if (memberId != createdBy)
            {
                members.Add(new ChatMember { ChatRoomId = chatRoom.Id, UserId = memberId });
            }
        }

        chatRoom.Members = members;

        _dbContext.ChatRooms.Add(chatRoom);
        await _dbContext.SaveChangesAsync();

        var savedRoom = await _dbContext.ChatRooms
            .Include(cr => cr.Members)
            .ThenInclude(m => m.User)
            .Include(cr => cr.Messages)
            .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(cr => cr.Id == chatRoom.Id);

        return MapToChatRoomDto(savedRoom!);
    }

    public async Task<ChatRoomDto> GetChatRoomAsync(Guid chatRoomId)
    {
        var chatRoom = await _dbContext.ChatRooms
            .Include(cr => cr.Members)
            .ThenInclude(m => m.User)
            .Include(cr => cr.Messages)
            .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(cr => cr.Id == chatRoomId);

        if (chatRoom == null)
            throw new InvalidOperationException("Chat room not found");

        return MapToChatRoomDto(chatRoom);
    }

    public async Task<List<ChatRoomDto>> GetUserChatRoomsAsync(Guid userId)
    {
        var chatRooms = await _dbContext.ChatRooms
            .Include(cr => cr.Members)
            .ThenInclude(m => m.User)
            .Include(cr => cr.Messages)
            .ThenInclude(m => m.Sender)
            .Where(cr => cr.Members.Any(m => m.UserId == userId))
            .OrderByDescending(cr => cr.CreatedAt)
            .ToListAsync();

        var result = new List<ChatRoomDto>();
        foreach (var chatRoom in chatRooms)
        {
            result.Add(MapToChatRoomDto(chatRoom));
        }

        return result;
    }

    public async Task<MessageDto> SendMessageAsync(Guid chatRoomId, Guid senderId, string text)
    {
        var chatRoom = await _dbContext.ChatRooms
            .Include(cr => cr.Members)
            .FirstOrDefaultAsync(cr => cr.Id == chatRoomId);

        if (chatRoom == null)
            throw new InvalidOperationException("Chat room not found");

        if (!chatRoom.Members.Any(m => m.UserId == senderId))
            throw new InvalidOperationException("User is not a member of this chat room");

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatRoomId = chatRoomId,
            SenderId = senderId,
            Text = text,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync();

        var savedMessage = await _dbContext.Messages
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == message.Id);

        return new MessageDto
        {
            Id = savedMessage!.Id,
            ChatRoomId = savedMessage.ChatRoomId,
            SenderId = savedMessage.SenderId,
            SenderUsername = savedMessage.Sender.Username,
            Text = savedMessage.Text,
            CreatedAt = savedMessage.CreatedAt
        };
    }

    public async Task<List<MessageDto>> GetChatMessagesAsync(Guid chatRoomId, int limit = 50)
    {
        var messages = await _dbContext.Messages
            .Include(m => m.Sender)
            .Where(m => m.ChatRoomId == chatRoomId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return messages.Select(m => new MessageDto
        {
            Id = m.Id,
            ChatRoomId = m.ChatRoomId,
            SenderId = m.SenderId,
            SenderUsername = m.Sender.Username,
            Text = m.Text,
            CreatedAt = m.CreatedAt
        }).ToList();
    }

    public async Task<bool> DeleteChatRoomAsync(Guid chatRoomId, Guid userId)
    {
        var chatRoom = await _dbContext.ChatRooms
            .FirstOrDefaultAsync(cr => cr.Id == chatRoomId);

        if (chatRoom == null)
            throw new InvalidOperationException("Chat room not found");

        if (chatRoom.CreatedByUserId != userId)
            throw new InvalidOperationException("Only chat room creator can delete it");

        _dbContext.ChatRooms.Remove(chatRoom);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AddUserToChatRoomAsync(Guid chatRoomId, string username, Guid invitedBy)
    {
        var chatRoom = await _dbContext.ChatRooms
            .Include(cr => cr.Members)
            .FirstOrDefaultAsync(cr => cr.Id == chatRoomId);

        if (chatRoom == null)
            throw new InvalidOperationException("Chat room not found");

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
            throw new InvalidOperationException("User not found");

        if (chatRoom.Members.Any(m => m.UserId == user.id))
            throw new InvalidOperationException("User is already a member of this chat room");

        var chatMember = new ChatMember { ChatRoomId = chatRoomId, UserId = user.id };
        chatRoom.Members.Add(chatMember);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<string?> RemoveUserFromChatRoomAsync(Guid chatRoomId, Guid userId)
    {
        var chatRoom = await _dbContext.ChatRooms
            .Include(cr => cr.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(cr => cr.Id == chatRoomId);

        if (chatRoom == null)
            return null;

        var member = chatRoom.Members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            return null;

        var username = member.User?.Username;
        _dbContext.ChatMembers.Remove(member);
        await _dbContext.SaveChangesAsync();

        return username;
    }

    private ChatRoomDto MapToChatRoomDto(ChatRoom chatRoom)
    {
        return new ChatRoomDto
        {
            Id = chatRoom.Id,
            Type = chatRoom.Type,
            Title = chatRoom.Title,
            CreatedAt = chatRoom.CreatedAt,
            CreatedByUserId = chatRoom.CreatedByUserId,
            MemberUsernames = chatRoom.Members.Select(m => m.User.Username).ToList(),
            Messages = chatRoom.Messages.Select(m => new MessageDto
            {
                Id = m.Id,
                ChatRoomId = m.ChatRoomId,
                SenderId = m.SenderId,
                SenderUsername = m.Sender.Username,
                Text = m.Text,
                CreatedAt = m.CreatedAt
            }).ToList()
        };
    }
}
