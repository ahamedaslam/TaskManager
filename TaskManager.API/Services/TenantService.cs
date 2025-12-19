using TaskManager.DTOs.TaskManager;
using TaskManager.Helper;
using TaskManager.Interface;
using TaskManager.IServices;
using TaskManager.Models;
using TaskManager.Models.Response;

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

        public async Task<Response> CreateTenantAsync(CreateTenantDTO request, string logId)
        {
            try
            {
                _logger.LogInformation("[{logId}] Received request to create tenant with Name: {TenantName}", logId, request.Name);
                var exists = await _tenantRepository.ExistsAsync(request.Name);
                if (exists)
                {
                    return ResponseHelper.BadRequest("Tenant with this name already exists.");
                }

                var newTenant = new Tenant
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = request.Name
 
                };
                 
                var result = await _tenantRepository.CreateAsync(newTenant);
                return ResponseHelper.Success(result,"Tenant created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{logId}] Error while creating tenant.",logId);
                 throw;
            }
        }

        public async Task<Response> GetAllTenantsAsync(string logId)
        {
            try
            {
                var tenants = await _tenantRepository.GetAllAsync();
                return ResponseHelper.Success(tenants, "Teanats Retrieved Successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching tenants.");
            return ResponseHelper.ServerError();
            }
        }

    }
}
