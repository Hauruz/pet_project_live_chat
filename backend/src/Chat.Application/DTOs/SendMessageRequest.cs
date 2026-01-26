namespace Chat.Application.DTOs;

public class SendMessageRequest
{
    public Guid ChatRoomId { get; set; }
    public string Text { get; set; } = null!;
}
