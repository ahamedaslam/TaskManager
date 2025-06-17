using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Models
{
    public class TaskItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueTime { get; set; }
        public bool IsCompleted { get; set; }
        public TaskPriority Priority { get; set; } // e.g., "Low", "Medium", "High"

        // Foreign key to Tenant
        public string TenantId { get; set; }

        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; }  // Add this navigation property

        
        
        // Foreign key to ApplicationUser
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
