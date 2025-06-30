using TaskManager.Models;

namespace TaskManager.Interface
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> GenerateAsync(ApplicationUser user);
        Task<RefreshToken> GetByTokenAsync(string token);

        Task InvalidateAsync(RefreshToken token);
    }
}
