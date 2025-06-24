using TaskManager.DTOs.DashBoard;

namespace TaskManager.Interface
{
    public interface IDashboardRepository
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync(string tenantId);
    }
}
