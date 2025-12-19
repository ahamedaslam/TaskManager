using TaskManager.Models.Response;

namespace TaskManager.Services.Interfaces
{
    public interface IAIChatService
    {
        Task<Response> GetAIResponseAsync(string message,string tenantId,string userId,string logId);
    }
}
