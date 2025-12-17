using Microsoft.EntityFrameworkCore;
using System.Text;
using TaskManager.DBContext;
using TaskManager.Helper;
using TaskManager.Models;
using TaskManager.Models.Response;
using TaskManager.Services.Interfaces;

namespace TaskManager.Services
{
    public class AIChatService : IAIChatService
    {
        private readonly AuthDBContext _dBContext;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AIChatService> _logger;
   

        public AIChatService(AuthDBContext dBContext, HttpClient httpClient,ILogger<AIChatService> logger)
        {
            _dBContext = dBContext;
            _httpClient = httpClient;
            _logger = logger;

        }

        public async Task<string> GetAIResponseAsync(string message, string tenantId, string userId)
        {
            _logger.LogInformation(
                "AI chat request started. UserId={UserId}, TenantId={TenantId}",
                userId, tenantId);


            // Load chat history
            // 1️ Load chat history

            _logger.LogDebug("Loading last 10 chat messages");

            var history = await _dBContext.ChatHistories
                .Where(c => c.UserId == userId && c.TenantId == tenantId)
                .OrderByDescending(c => c.CreatedAt)
                .Take(10)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            _logger.LogInformation("Loaded {Count} chat history records", history.Count);

            // Load tasks
            _logger.LogDebug("Loading user tasks");

            var tasks = await _dBContext.TaskItems
                .Where(t => t.UserId == userId && t.TenantId == tenantId)
                .Select(t => $"{t.Title} - {(t.IsCompleted ? "Completed" : "Pending")}")
                .ToListAsync();

            _logger.LogInformation("Loaded {Count} tasks for AI context", tasks.Count);

            //  Build prompt
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("User Tasks:");
            foreach (var task in tasks)
                promptBuilder.AppendLine($"- {task}");

            promptBuilder.AppendLine("\nConversation:");
            foreach (var msg in history)
                promptBuilder.AppendLine($"{msg.Role}: {msg.Message}");

            promptBuilder.AppendLine($"user: {message}");
            promptBuilder.AppendLine("assistant:");

            _logger.LogDebug("Prompt built successfully");

            var payload = new
            {
                model = "deepseek-v3.1:671b-cloud",
                prompt = promptBuilder.ToString(),
                stream = false
            };

            // Call Ollama
            _logger.LogInformation("Sending request to Ollama");

            var response = await _httpClient.PostAsJsonAsync("http://localhost:11434/api/generate", payload);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Ollama call failed. StatusCode={StatusCode}",
                    response.StatusCode);

                throw new ApplicationException("AI service failed");
            }

            var rawJson = await response.Content.ReadAsStringAsync();

            var ollamaResult = System.Text.Json.JsonSerializer.Deserialize<AiResponse>( rawJson,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            var aiMessage = CleanAi.CleanAiText(ollamaResult?.Response ?? "No response from AI");

            _logger.LogInformation("AI response received successfully");

            // Save chat history
            _logger.LogDebug("Saving chat history to database");

            _dBContext.ChatHistories.AddRange(
                new ChatHistory
                {
                    UserId = userId,
                    TenantId = tenantId,
                    Role = "user",
                    Message = message,
                    CreatedAt = DateTime.UtcNow
                },
                new ChatHistory
                {
                    UserId = userId,
                    TenantId = tenantId,
                    Role = "assistant",
                    Message = aiMessage,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await _dBContext.SaveChangesAsync();

            _logger.LogInformation(
                "Chat history saved successfully. UserId={UserId}",
                userId);

            return aiMessage;
        }
    }
}
