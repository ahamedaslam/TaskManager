using TaskManager.DTOs.DashBoard;
using TaskManager.Helper;
using TaskManager.Interface;
using TaskManager.Models.Responses;
using TaskManager.Services.Interfaces;

public class DashboardService : IDashboardService
{
    private readonly ILogger<DashboardService> _logger;
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(ILogger<DashboardService> logger, IDashboardRepository dashboardRepository)
    {
        _logger = logger;
        _dashboardRepository = dashboardRepository;
    }

    public async Task<Response<DashboardStatsDto>> GetDashboardStatsAsync(string tenantId,string logId)
    {
       
        _logger.LogInformation("[{LogId}] Fetching dashboard stats for TenantId: {TenantId}", logId, tenantId);

        try
        {
            var stats = await _dashboardRepository.GetDashboardStatsAsync(tenantId);

            if (stats == null)
            {
                _logger.LogWarning("[{LogId}] No dashboard stats found for TenantId: {TenantId}", logId, tenantId);
                return ResponseHelper.NotFound<DashboardStatsDto>("No dashboard stats found.");
            }

            _logger.LogInformation("[{LogId}] Successfully fetched dashboard stats for TenantId: {TenantId}", logId, tenantId);
            return ResponseHelper.SuccessGeneric<DashboardStatsDto>(stats, "Dashboard stats fetched successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{LogId}] Exception while fetching dashboard stats for TenantId: {TenantId}", logId, tenantId);
            return ResponseHelper.ServerError<DashboardStatsDto>();
        }
    }
}
