using JwtOAuthDemo.API.Services;
using JwtOAuthDemo.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Mvc;

namespace JwtOAuthDemo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error");
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration error");
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Invalid refresh token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refresh token error");
            return StatusCode(500, new { message = "An error occurred during token refresh" });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var result = await _authService.LogoutAsync(request.RefreshToken);
            return Ok(new { success = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout error");
            return StatusCode(500, new { message = "An error occurred during logout" });
        }
    }

    [HttpGet("oauth/google")]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "http://localhost:3000/oauth-callback"
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("oauth/facebook")]
    public IActionResult FacebookLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "http://localhost:3000/oauth-callback"
        };
        return Challenge(properties, FacebookDefaults.AuthenticationScheme);
    }

    [HttpGet("oauth/callback")]
    public async Task<IActionResult> OAuthCallback()
    {
        var authenticateResult = await HttpContext.AuthenticateAsync();

        if (!authenticateResult.Succeeded)
        {
            return BadRequest(new { message = "OAuth authentication failed" });
        }

        var claims = authenticateResult.Principal?.Claims;
        var provider = authenticateResult.Properties?.Items[".AuthScheme"] ?? "unknown";
        var providerId = claims?.FirstOrDefault(c => c.Type == "sub")?.Value ?? 
                        claims?.FirstOrDefault(c => c.Type == "id")?.Value ?? 
                        claims?.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        var email = claims?.FirstOrDefault(c => c.Type == "email")?.Value ?? string.Empty;
        var username = claims?.FirstOrDefault(c => c.Type == "name")?.Value ?? 
                       claims?.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;

        try
        {
            var response = await _authService.HandleOAuthLoginAsync(provider, providerId ?? string.Empty, email, username ?? email);
            
            // Return to frontend with token
            return Redirect($"http://localhost:3000/oauth-callback?token={response.AccessToken}&refreshToken={response.RefreshToken}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth callback error");
            return Redirect($"http://localhost:3000/oauth-callback?error={ex.Message}");
        }
    }
}