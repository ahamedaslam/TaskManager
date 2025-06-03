using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.DBContext
{
    public class AuthDBContext : IdentityDbContext
    {
        public AuthDBContext(DbContextOptions<AuthDBContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var adminRoleId = "55e14e82-8ef2-44d0-8f75-be74a4bdfa5f";
            var normalRoleId = "2a0c839e-ded6-4715-b8bd-53a514cb3a26"; // Same ID, just new name

            var roles = new List<IdentityRole>
                {
                new IdentityRole
                {
                Id = adminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                Id = normalRoleId,
                Name = "Normal",             // Changed from "User" to "Normal"
                NormalizedName = "NORMAL"   // Changed from "USER" to "NORMAL"
                }
        };
            //•	builder.Entity<IdentityRole>().HasData(roles); tells EF Core to insert these roles into the database if they don’t already exist.

            builder.Entity<IdentityRole>().HasData(roles);
        }


    }
}