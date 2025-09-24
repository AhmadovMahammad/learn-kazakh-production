using LearnKazakh.Domain.Entities;
using LearnKazakh.Infrastructure.Authentication;
using LearnKazakh.Persistence;
using LearnKazakh.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LearnKazakh.API.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController
(
    LearnKazakhContext context,
    IJwtService jwtService,
    IPasswordService passwordService,
    IRefreshTokenService refreshTokenService
) : ControllerBase
{
    private readonly LearnKazakhContext _context = context;
    private readonly IJwtService _jwtService = jwtService;
    private readonly IPasswordService _passwordService = passwordService;
    private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Register([FromBody] RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(ApiResponse<LoginResponse>.ErrorResult
            (
                new InvalidOperationException("Email already registered"),
                "Email already registered")
            );
        }

        var (hash, salt) = _passwordService.HashPassword(request.Password);

        Domain.Entities.User user = new()
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow,
            UserRoles =
            [
                new UserRole { Role = new Role { Name = "User" } }
            ]
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        string accessToken = _jwtService.GenerateAccessToken(user);
        RefreshToken refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

        LoginResponse response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            UserProfileDto = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                LastLoginAt = DateTime.UtcNow,
            }
        };

        return Ok(ApiResponse<LoginResponse>.SuccessResult(response, "User registered successfully"));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        Domain.Entities.User? user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return Unauthorized(ApiResponse<LoginResponse>.ErrorResult
            (
                new UnauthorizedAccessException("Invalid credentials"),
                "Invalid email or password")
            );
        }

        string accessToken = _jwtService.GenerateAccessToken(user);
        RefreshToken refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

        LoginResponse response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            UserProfileDto = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                LastLoginAt = DateTime.UtcNow,
            }
        };

        return Ok(ApiResponse<LoginResponse>.SuccessResult(response, "Logged in successfully"));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var oldToken = await _refreshTokenService.GetRefreshTokenAsync(request.RefreshToken);
        if (oldToken == null || !oldToken.IsActive)
        {
            return Unauthorized(ApiResponse<RefreshTokenResponse>.ErrorResult
            (
                new SecurityTokenException("Invalid refresh token"),
                "Refresh token invalid or expired")
            );
        }

        Domain.Entities.User? user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == oldToken.UserId);

        if (user == null)
        {
            return Unauthorized(ApiResponse<RefreshTokenResponse>.ErrorResult
            (
                new KeyNotFoundException("User not found"),
                "User not found")
            );
        }

        string newAccessToken = _jwtService.GenerateAccessToken(user);
        RefreshToken newRefreshToken = await _refreshTokenService.RotateRefreshTokenAsync(oldToken, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

        RefreshTokenResponse response = new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        return Ok(ApiResponse<RefreshTokenResponse>.SuccessResult(response, "Token refreshed successfully"));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout([FromBody] RefreshTokenRequest request)
    {
        await _refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", "User logged out");
        return Ok(ApiResponse<object>.SuccessResult(new object(), "Logged out successfully"));
    }
}
