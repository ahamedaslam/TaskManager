using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.Models;

namespace TaskManager.DBContext
{
    // Remove any DbSet<IdentityUser> or direct references to IdentityUser from your DbContext.
    // Ensure you do NOT have a DbSet<IdentityUser> property in AuthDBContext.
    // The following is correct and does NOT include a DbSet<IdentityUser>:

    public class AuthDBContext : IdentityDbContext<ApplicationUser> //For navigation to ApplicationUser which extends IdentityUser
    {
        public AuthDBContext(DbContextOptions<AuthDBContext> options) : base(options)
        {
        }

        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<ChatHistory> ChatHistories { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
                    
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Tenant)
                .WithMany()
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            var adminRoleId = "55e14e82-8ef2-44d0-8f75-be74a4bdfa5f";  //Guid.NewGuid().ToString();
            var normalRoleId = "2a0c839e-ded6-4715-b8bd-53a514cb3a26";
            
            
            builder.Entity<ChatHistory>()
                .HasOne(ch => ch.User)
                .WithMany()
                .HasForeignKey(ch => ch.UserId);


            var roles = new List<IdentityRole> //sed up initial roles
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
                    Name = "Normal",
                    NormalizedName = "NORMAL"
                }
            };

            builder.Entity<IdentityRole>().HasData(roles);
        }
    }
}