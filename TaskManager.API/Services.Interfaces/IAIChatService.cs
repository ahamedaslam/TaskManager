namespace TaskManager.Services.Interfaces
{
    public interface IAIChatService
    {
        Task<string> GetAIResponseAsync(string message,string tenantId,string userId);
    }
}
