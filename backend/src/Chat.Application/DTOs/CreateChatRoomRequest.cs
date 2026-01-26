using System.Text.Json.Serialization;

namespace Chat.Application.DTOs;

public class CreateChatRoomRequest
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Direct";

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("memberIds")]
    public List<Guid> MemberIds { get; set; } = new();
}
