using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.DTOs.TaskManager;
using TaskManager.Helper;
using TaskManager.IServices;

namespace TaskManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly ILogger<TenantController> _logger;

        public TenantController(ITenantService tenantService, ILogger<TenantController> logger)
        {
            _tenantService = tenantService;
            _logger = logger;
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantDTO request)
        {
            var logId = Guid.NewGuid().ToString();
            _logger.LogInformation("[{logId}] Received request to create tenant with Name: {TenantName}", logId, request.Name);
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                _logger.LogWarning("[{logId}] Invalid request: {Request}", logId, request);
                return BadRequest(ResponseHelper.BadRequest("Invalid request data."));
            }
            _logger.LogDebug("[{logId}] Creating tenant with Name: {TenantName}", logId, request.Name);
            var response = await _tenantService.CreateTenantAsync(request, logId);
            _logger.LogInformation("[{logId}] result CreateTenant response: {@Response}", logId, response);
            return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
        }
           
            
        

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTenants()
        {
            var logId = Guid.NewGuid().ToString();

            _logger.LogInformation("[{logId}] Received request to get all tenants", logId);
            var response = await _tenantService.GetAllTenantsAsync(logId);
            _logger.LogInformation("[{logId}] GetAllTenants response: {@Response}", logId, response);
            return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
        }
    }
}
