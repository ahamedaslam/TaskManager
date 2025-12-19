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


        public AIChatService(AuthDBContext dBContext, HttpClient httpClient, ILogger<AIChatService> logger)
        {
            _dBContext = dBContext;
            _httpClient = httpClient;
            _logger = logger;

        }

        public async Task<Response> GetAIResponseAsync(string message, string tenantId, string userId,string logId)
        {
            try
            {
                _logger.LogInformation("[{logId}] AI chat request started. UserId={UserId}, TenantId={TenantId}", logId,userId, tenantId);
                
                // Load chat history
                // Load chat history

                _logger.LogDebug("[{logId}] Loading last 10 chat messages",logId);

                var history = await _dBContext.ChatHistories
                    .Where(c => c.UserId == userId && c.TenantId == tenantId)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(10)
                    .OrderBy(c => c.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("[{logId}] Loaded {Count} chat history records", logId, history.Count);

                // Load tasks
                _logger.LogDebug("[{logId}] Loading user tasks", logId);

                var tasks = await _dBContext.TaskItems
                    .Where(t => t.UserId == userId && t.TenantId == tenantId)
                    .Select(t => $"{t.Title} - {(t.IsCompleted ? "Completed" : "Pending")}")
                    .ToListAsync();

                _logger.LogInformation("[{logId}] Loaded {Count} tasks for AI context", logId, tasks.Count);

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

                _logger.LogDebug("[{logId}] Prompt built successfully", logId);

                var payload = new
                {
                    model = "deepseek-v3.1:671b-cloud",
                    prompt = promptBuilder.ToString(),
                    stream = false
                };

                // Call Ollama
                _logger.LogInformation("[{logId}] Sending request to Ollama", logId);

                var response = await _httpClient.PostAsJsonAsync("http://localhost:11434/api/generate", payload);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("[{logId}] Ollama call failed. StatusCode={StatusCode}", logId, response.StatusCode);

                    throw new ApplicationException("AI service failed");
                }

                var rawJson = await response.Content.ReadAsStringAsync();

                var ollamaResult = System.Text.Json.JsonSerializer.Deserialize<AiResponse>(rawJson,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                var aiMessage = CleanAi.CleanAiText(ollamaResult?.Response ?? "No response from AI");

                _logger.LogInformation("[{logId}] AI response received successfully", logId);

                // Save chat history
                _logger.LogDebug("[{logId}] Saving chat history to database", logId);

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

                _logger.LogInformation("[{logId}] Chat history saved successfully. UserId={UserId}", logId,userId);

                return new Response
                {
                    ResponseCode = 0,
                    ResponseDescription = "Success",
                    ResponseDatas = aiMessage
                };
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
