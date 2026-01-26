namespace Chat.Domain.Entities
{
    public class ChatRoom
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = "Direct";
        public string? Title { get; set; }
        public Guid CreatedByUserId { get; set; }
        public User? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<ChatMember> Members { get; set; } = new();
        public List<Message> Messages { get; set; } = new();
    }
}