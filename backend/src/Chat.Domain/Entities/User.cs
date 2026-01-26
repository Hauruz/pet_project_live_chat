namespace Chat.Domain.Entities
{
    public class User
    {
        public Guid id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<ChatMember> ChatMemberships { get; set; } = new();
        public List<Message> Messages { get; set; } = new();

    }
}