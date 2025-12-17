using Microsoft.AspNetCore.Mvc;
using TaskManager.Helper;
using TaskManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace TaskManager.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;
        private readonly CurrentUserService _currentUserService;
        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger,CurrentUserService currentUserService)
        {
            _dashboardService = dashboardService;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var logId = Guid.NewGuid().ToString();
            try
            {
                var tenantId = _currentUserService.GetTenantId;

                if (string.IsNullOrEmpty(tenantId))
                {
                    _logger.LogWarning("[{LogId}] Missing TenantId in claims.", logId);
                    return Unauthorized(ResponseHelper.Unauthorized("TenantId not found in claims."));
                }
                _logger.LogDebug("[{LogId}] Entering GetDashboardStats with TenantId: {TenantId}", logId, tenantId);
                var result = await _dashboardService.GetDashboardStatsAsync(tenantId,logId);

                if (result == null)
                {
                    _logger.LogWarning("[{LogId}] No dashboard stats found for TenantId: {TenantId}", logId, tenantId);
                    return NotFound(ResponseHelper.NotFound($"No dashboard stats found for TenantId: {tenantId}"));
                }

                _logger.LogInformation("[{LogId}] Successfully fetched dashboard stats for TenantId: {TenantId}", logId, tenantId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var error = ResponseHelper.ServerError(logId);
                return StatusCode(HttpStatusMapper.GetHttpStatusCode(error.ResponseCode), error);

            }
        }
    } 
}
