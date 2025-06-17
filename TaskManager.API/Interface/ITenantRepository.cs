using TaskManager.Models;

namespace TaskManager.Interface
{
    public interface ITenantRepository
    {
        Task<Tenant> CreateAsync(Tenant tenant);
        Task<bool> ExistsAsync(string name);

        Task<IEnumerable<Tenant>> GetAllAsync();

        //Task<IEnumerable<Tenant>> GetAllByTenantIdAsync(string tenantId);
    }
}
