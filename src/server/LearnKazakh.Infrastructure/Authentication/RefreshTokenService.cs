using LearnKazakh.Domain.Entities;
using LearnKazakh.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace LearnKazakh.Infrastructure.Authentication;

public interface IRefreshTokenService
{
    Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string ipAddress);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task<bool> ValidateRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token, string ipAddress, string reason);
    Task<RefreshToken> RotateRefreshTokenAsync(RefreshToken oldToken, string ipAddress);
    Task CleanupExpiredTokensAsync();
}

public class RefreshTokenService : IRefreshTokenService
{
    private readonly LearnKazakhContext _learnKazakhContext;
    private readonly int _refreshTokenExpirationDays = 1;

    public RefreshTokenService(LearnKazakhContext learnKazakhContext, IConfiguration configuration)
    {
        _learnKazakhContext = learnKazakhContext;
        if (int.TryParse(configuration["Authentication:Jwt:RefreshTokenExpirationDays"], out int expirationDays))
        {
            _refreshTokenExpirationDays = expirationDays;
        }
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string ipAddress)
    {
        RefreshToken refreshToken = new RefreshToken
        {
            Token = GenerateRefreshToken(),
            UserId = userId,
            CreatedByIp = ipAddress,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays),
        };

        _learnKazakhContext.RefreshTokens.Add(refreshToken);
        await _learnKazakhContext.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _learnKazakhContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<bool> ValidateRefreshTokenAsync(string token)
    {
        var refreshToken = await GetRefreshTokenAsync(token);
        return refreshToken != null && refreshToken.IsActive;
    }

    public async Task RevokeRefreshTokenAsync(string token, string ipAddress, string reason)
    {
        var refreshToken = await GetRefreshTokenAsync(token);
        if (refreshToken != null && refreshToken.IsActive)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.RevokedReason = reason;

            await _learnKazakhContext.SaveChangesAsync();
        }
    }

    public async Task<RefreshToken> RotateRefreshTokenAsync(RefreshToken oldToken, string ipAddress)
    {
        await RevokeRefreshTokenAsync(oldToken.Token, ipAddress, "TOKEN ROTATION");
        var newToken = await GenerateRefreshTokenAsync(oldToken.UserId, ipAddress);
        oldToken.ReplacedByToken = newToken.Token;

        _learnKazakhContext.RefreshTokens.Add(newToken);
        await _learnKazakhContext.SaveChangesAsync();

        return newToken;
    }

    public async Task CleanupExpiredTokensAsync()
    {
        var expiredTokens = _learnKazakhContext.RefreshTokens
            .Where(rt => rt.IsActive)
            .ToList();

        if (expiredTokens.Count != 0)
        {
            _learnKazakhContext.RemoveRange(expiredTokens);
            await _learnKazakhContext.SaveChangesAsync();
        }
    }

    // helper
    private string GenerateRefreshToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
