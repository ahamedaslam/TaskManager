using TaskManager.DTOs.TaskManager;
using TaskManager.Interface;
using TaskManager.IServices;
using TaskManager.Models;

namespace TaskManager.Services
{
    public class TenantService : ITenantService
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly ILogger<TenantService> _logger;

        public TenantService(ITenantRepository tenantRepository, ILogger<TenantService> logger)
        {
            _tenantRepository = tenantRepository;
            _logger = logger;
        }

        public async Task<Response> CreateTenantAsync(CreateTenantDTO request)
        {
            try
            {
                var exists = await _tenantRepository.ExistsAsync(request.Name);
                if (exists)
                {
                    return new Response
                    {
                        ResponseCode = 1001,
                        ResponseDescription = "Tenant with the same name already exists."
                    };
                }

                var newTenant = new Tenant
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = request.Name
 
                };

                var result = await _tenantRepository.CreateAsync(newTenant);

                return new Response
                {
                    ResponseCode = 0,
                    ResponseDescription = "Tenant created successfully.",
                    ResponseDatas = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating tenant.");
                return new Response
                {
                    ResponseCode = 1006,
                    ResponseDescription = "Internal server error."
                };
            }
        }

        public async Task<Response> GetAllTenantsAsync()
        {
            try
            {
                var tenants = await _tenantRepository.GetAllAsync();
                return new Response
                {
                    ResponseCode = 0,
                    ResponseDescription = "Tenants fetched successfully.",
                    ResponseDatas = tenants
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching tenants.");
                return new Response
                {
                    ResponseCode = 500,
                    ResponseDescription = "Internal server error."
                };
            }
        }

    }
}
