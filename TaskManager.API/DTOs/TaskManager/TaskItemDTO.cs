using TaskManager.Models;

namespace TaskManager.DTOs.TaskManager
{
    public class TaskItemDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueTime { get; set; }
        public bool IsCompleted { get; set; }
        public TaskPriority Priority { get; set; }
        public string TenantId { get; set; } // Foreign key to Tenant

        public string UserId { get; set; }
    }
}
