using Chat.Application.DTOs;
using Chat.Application.Services;
using Chat.Domain.Entities;
using Chat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Chat.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ChatDbContext _dbContext;
    private readonly IJwtService _jwtService;

    public AuthService(ChatDbContext dbContext, IJwtService jwtService)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await UserExistsAsync(request.Username))
            throw new InvalidOperationException("User already exists");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new InvalidOperationException("Password cannot be empty");

        var user = new User
        {
            id = Guid.NewGuid(),
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user.id, user.Username);

        return new AuthResponse
        {
            Token = token,
            Username = user.Username,
            UserId = user.id
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
            throw new InvalidOperationException("Invalid credentials");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new InvalidOperationException("Invalid credentials");

        var token = _jwtService.GenerateToken(user.id, user.Username);

        return new AuthResponse
        {
            Token = token,
            Username = user.Username,
            UserId = user.id
        };
    }

    public async Task<bool> UserExistsAsync(string username)
    {
        return await _dbContext.Users
            .AnyAsync(u => u.Username == username);
    }
}
