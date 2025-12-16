using Microsoft.AspNetCore.Identity;

namespace TaskManager.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string TenantId { get; set; } // Foreign key to Tenant
        public Tenant Tenant { get; set; } // Navigation property to Tenant


      //properties for OTP and its expiry
        public string?  OTP { get; set; }
        public DateTime? OTPExpiry { get; set; }

    }

}
