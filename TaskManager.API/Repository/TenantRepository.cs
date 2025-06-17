using Microsoft.EntityFrameworkCore;
using System;
using TaskManager.DBContext;
using TaskManager.Interface;
using TaskManager.Models;

namespace TaskManager.Repository
{
    public class TenantRepository : ITenantRepository
    {
        private readonly AuthDBContext _context;

        public TenantRepository(AuthDBContext context)
        {
            _context = context;
        }

        public async Task<Tenant> CreateAsync(Tenant tenant)
        {
            await _context.Tenants.AddAsync(tenant);
            await _context.SaveChangesAsync();
            return tenant;
        }

        public async  Task<bool> ExistsAsync(string name)
        {
            return await _context.Tenants.AnyAsync(t => t.Name == name);
        }

        public async Task<IEnumerable<Tenant>> GetAllAsync()
        {
            return await _context.Tenants.ToListAsync();
        }

        //public async Task<IEnumerable<Tenant>> GetAllByTenantIdAsync(string tenantId)
        //{
        //    return await _context.Tenants.Where(t => t.Id == tenantId).ToListAsync();
        //}
    }
}
