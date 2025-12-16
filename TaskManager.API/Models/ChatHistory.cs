namespace TaskManager.Models
{
    public class ChatHistory
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty; //FK to User 

        public string TenantId { get; set; } = string.Empty;

        // "user" or "assistant"
        public string Role { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ApplicationUser User { get; set; }
        public Tenant Tenant { get; set; }

    }
}
