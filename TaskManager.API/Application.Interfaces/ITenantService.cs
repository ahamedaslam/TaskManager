using TaskManager.DTOs.TaskManager;
using TaskManager.Models.Response;

namespace TaskManager.IServices
{
    public interface ITenantService
    {
        Task<Response> CreateTenantAsync(CreateTenantDTO request,string logId);
        Task<Response> GetAllTenantsAsync(string logId);

        //Task<Response> GetAllTenantsByTenantIdAsync(string tenantId);
    }
}
