using Microsoft.AspNetCore.Mvc;
using TaskManager.DTOs.AIChat;
using TaskManager.Models.Response;
using TaskManager.Services.Interfaces;

namespace TaskManager.Controllers
{
    [Route("api/ai")]
    [ApiController]
    public class AIChatController : ControllerBase
    {
        private readonly CurrentUserService _currentUserService;
        private readonly IAIChatService _aiChatService;
        private readonly ILogger<AIChatController> _logger;

        public AIChatController(
            CurrentUserService currentUserService,
            IAIChatService aiChatService,
            ILogger<AIChatController> logger)
        {
            _currentUserService = currentUserService;
            _aiChatService = aiChatService;
            _logger = logger;
        }

        [HttpPost("chat")]
        public async Task<ActionResult<Response>> Chat([FromBody] ChatRequest request)
        {
            _logger.LogInformation("AI Chat request received");

            var userId = _currentUserService.GetUserId;
            var tenantId = _currentUserService.GetTenantId;

            _logger.LogInformation(
                "UserId: {UserId}, TenantId: {TenantId}, Message: {Message}",
                userId,
                tenantId,
                request.message
            );

            var aiResult = await _aiChatService.GetAIResponseAsync(
                request.message,
                tenantId,
                userId
            );

            _logger.LogInformation(
                "AI Chat response sent for UserId: {UserId}, TenantId: {TenantId}",
                userId,
                tenantId
            );

            return new Response
            {
                ResponseCode = 0,
                ResponseDescription = "Success",
                ResponseDatas = aiResult
            };
        }
    }
}
