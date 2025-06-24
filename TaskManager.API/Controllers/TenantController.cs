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
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantDTO request)
        {

           
            _logger.LogInformation("Received request to create tenant with Name: {TenantName}", request.Name);
            var response = await _tenantService.CreateTenantAsync(request);
            _logger.LogInformation("CreateTenant response: {@Response}", response);
            return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTenants()
        {
            _logger.LogInformation("Received request to get all tenants");
            var response = await _tenantService.GetAllTenantsAsync();
            _logger.LogInformation("GetAllTenants response: {@Response}", response);
            return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
        }
    }
}
