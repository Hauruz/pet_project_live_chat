namespace Chat.Application.Services;

public interface IJwtService
{
    string GenerateToken(Guid userId, string username);
    (Guid UserId, string Username) ValidateToken(string token);
}
