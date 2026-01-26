using System.Text.Json.Serialization;

namespace Chat.Application.DTOs;

public class MessageDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("chatRoomId")]
    public Guid ChatRoomId { get; set; }

    [JsonPropertyName("senderId")]
    public Guid SenderId { get; set; }

    [JsonPropertyName("senderUsername")]
    public string SenderUsername { get; set; } = null!;

    [JsonPropertyName("text")]
    public string Text { get; set; } = null!;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}
