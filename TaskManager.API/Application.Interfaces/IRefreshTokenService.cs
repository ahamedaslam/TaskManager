using TaskManager.Models;
using TaskManager.Models.Response;

namespace TaskManager.Services.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<Response> GenerateTokensAsync(ApplicationUser user);
        Task<Response> RefreshAsync(string accessToken, string refreshToken,string logId);
    }
}
