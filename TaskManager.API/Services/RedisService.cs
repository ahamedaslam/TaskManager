using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

public class RedisService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisService> _logger;

    public RedisService(IDistributedCache cache, ILogger<RedisService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    // 1) Store Refresh Token
   
    public async Task StoreRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAt)
    {
        try
        {
            await _cache.SetStringAsync($"refreshToken:{refreshToken}",refreshToken,                        
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = expiresAt
                });

            _logger.LogInformation("Stored refresh token for UserId: {UserId}, ExpiresAt: {ExpiresAt}",userId,expiresAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing refresh token for UserId: {UserId}", userId);
            throw;
        }

    }

    // 2) Get Refresh Token
    public async Task<string?> GetRefreshTokenAsync(string userId)
    {
        try
        {
            var token = await _cache.GetStringAsync($"refresh:{userId}");

            if (token == null)
                _logger.LogWarning("Refresh token not found for UserId: {UserId}", userId);
            else
                _logger.LogInformation("Refresh token retrieved for UserId: {UserId}", userId);

            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving refresh token for UserId: {UserId}", userId);
            throw;
        }
    }

   
}
