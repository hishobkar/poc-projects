using JwtOAuthDemo.Core.Models;

namespace JwtOAuthDemo.API.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(string refreshToken);
    Task<AuthResponse> HandleOAuthLoginAsync(string provider, string providerId, string email, string username);
}