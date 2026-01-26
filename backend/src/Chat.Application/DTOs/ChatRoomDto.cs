using System.Text.Json.Serialization;

namespace Chat.Application.DTOs;

public class ChatRoomDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("createdByUserId")]
    public Guid CreatedByUserId { get; set; }

    [JsonPropertyName("memberUsernames")]
    public List<string> MemberUsernames { get; set; } = new();

    [JsonPropertyName("messages")]
    public List<MessageDto> Messages { get; set; } = new();
}
