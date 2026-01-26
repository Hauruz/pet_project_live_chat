using Chat.Application.DTOs;

namespace Chat.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<bool> UserExistsAsync(string username);
}
