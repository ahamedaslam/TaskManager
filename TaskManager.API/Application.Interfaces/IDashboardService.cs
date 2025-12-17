using TaskManager.DTOs.DashBoard;
using TaskManager.Models.Responses;

namespace TaskManager.Services.Interfaces
{
    public interface IDashboardService
    {
       public Task<Response<DashboardStatsDto>> GetDashboardStatsAsync(string tenantId, string logId);
    }
}
