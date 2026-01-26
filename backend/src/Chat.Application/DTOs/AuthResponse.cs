namespace Chat.Application.DTOs;

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public string Username { get; set; } = null!;
    public Guid UserId { get; set; }
}
