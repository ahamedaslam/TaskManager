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
            await _cache.SetStringAsync(
                $"refreshToken:{refreshToken}",      
                refreshToken,                        
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = expiresAt
                });

            _logger.LogInformation(
                "Stored refresh token for UserId: {UserId}, ExpiresAt: {ExpiresAt}",
                userId,
                expiresAt
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing refresh token for UserId: {UserId}", userId);
            throw;
        }

    }

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

    // 2) Blacklist JWT Token

    public async Task BlacklistTokenAsync(string jwtToken, DateTime expires)
    {
        try
        {
            await _cache.SetStringAsync(
                $"blacklist:{jwtToken}",
                "true",
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = expires
                });

            _logger.LogInformation("Token blacklisted. ExpiresAt: {ExpiresAt}", expires);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blacklisting token");
            throw;
        }
    }

    public async Task<bool> IsTokenBlacklistedAsync(string jwtToken)
    {
        try
        {
            var result = await _cache.GetStringAsync($"blacklist:{jwtToken}");

            bool isBlacklisted = result != null;

            _logger.LogInformation("Token {Token} blacklist status: {Status}", jwtToken, isBlacklisted);

            return isBlacklisted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking blacklist status for token");
            throw;
        }
    }

    // 3) Task Caching
    public async Task CacheTaskAsync<T>(string taskId, T taskData)
    {
        try
        {
            var json = JsonSerializer.Serialize(taskData);

            await _cache.SetStringAsync(
                $"task:{taskId}",
                json,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

            _logger.LogInformation("Cached task data for TaskId: {TaskId}", taskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching task TaskId: {TaskId}", taskId);
            throw;
        }
    }

    public async Task<T?> GetCachedTaskAsync<T>(string taskId)
    {
        try
        {
            var json = await _cache.GetStringAsync($"task:{taskId}");

            if (json == null)
            {
                _logger.LogWarning("Task cache miss for TaskId: {TaskId}", taskId);
                return default;
            }

            _logger.LogInformation("Task cache hit for TaskId: {TaskId}", taskId);

            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached task TaskId: {TaskId}", taskId);
            throw;
        }
    }

    public async Task RemoveCachedTaskAsync(string taskId)
    {
        try
        {
            await _cache.RemoveAsync($"task:{taskId}");
            _logger.LogInformation("Removed cached task for TaskId: {TaskId}", taskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached task TaskId: {TaskId}", taskId);
            throw;
        }
    }
}
