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

        public string UserId { get; set; }  // Foreign key to AspNetUsers

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; } // Navigation property to Identity user
    }
}
    