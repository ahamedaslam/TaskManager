using Microsoft.EntityFrameworkCore;
using TaskManager.DBContext;
using TaskManager.DTOs.DashBoard;
using TaskManager.Interface;

namespace TaskManager.Repository
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AuthDBContext _dbContext;


        public DashboardRepository(AuthDBContext authDBContext)
        {
            _dbContext = authDBContext;
        }
        public async Task<DashboardStatsDto> GetDashboardStatsAsync(string tenantId)
        {
            var totalTasks = await _dbContext.TaskItems.CountAsync(t => t.TenantId == tenantId);
            var completedTasks = await _dbContext.TaskItems.CountAsync(t => t.TenantId == tenantId && t.IsCompleted);
            var pendingTasks = totalTasks - completedTasks;

            return new DashboardStatsDto
            {
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                PendingTasks = pendingTasks
            };
        }
    }
}
