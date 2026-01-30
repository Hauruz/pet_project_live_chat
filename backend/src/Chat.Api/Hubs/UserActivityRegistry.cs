using System.Collections.Concurrent;

namespace Chat.Api.Hubs;

public class UserActivityInfo
{
    public string ConnectionId { get; init; } = null!;
    public Guid UserId { get; init; }
    public DateTime LastActivityUtc { get; set; } = DateTime.UtcNow;
    public HashSet<Guid> ChatRooms { get; } = new();
}

public class UserActivityRegistry
{
    private readonly ConcurrentDictionary<string, UserActivityInfo> _connections = new();

    public void Add(string connectionId, Guid userId)
    {
        _connections[connectionId] = new UserActivityInfo
        {
            ConnectionId = connectionId,
            UserId = userId,
            LastActivityUtc = DateTime.UtcNow
        };
    }

    public void Remove(string connectionId)
        => _connections.TryRemove(connectionId, out _);

    public void Touch(string connectionId)
    {
        if (_connections.TryGetValue(connectionId, out var info))
            info.LastActivityUtc = DateTime.UtcNow;
    }

    public void JoinChat(string connectionId, Guid chatId)
    {
        if (_connections.TryGetValue(connectionId, out var info))
            info.ChatRooms.Add(chatId);
    }

    public void LeaveChat(string connectionId, Guid chatId)
    {
        if (_connections.TryGetValue(connectionId, out var info))
            info.ChatRooms.Remove(chatId);
    }

    public IReadOnlyCollection<UserActivityInfo> Snapshot()
        => _connections.Values.ToList();
}
