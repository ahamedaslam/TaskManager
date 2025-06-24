using TaskManager.DTOs.TaskManager;
using TaskManager.Models.Response;

namespace TaskManager.IServices
{
    public interface ITenantService
    {
        Task<Response> CreateTenantAsync(CreateTenantDTO request);
        Task<Response> GetAllTenantsAsync();

        //Task<Response> GetAllTenantsByTenantIdAsync(string tenantId);
    }
}
