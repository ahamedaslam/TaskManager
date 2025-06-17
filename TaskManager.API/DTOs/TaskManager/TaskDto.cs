namespace TaskManager.DTOs.TaskManager
{
    public class TaskDto
    {
        public string userId { get; set; }
        public Guid taskId { get; set; }

        public string tenantId { get; set; } // Foreign key to Tenant

    }
}
