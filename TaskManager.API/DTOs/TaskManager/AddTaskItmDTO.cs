using TaskManager.Models;

namespace TaskManager.DTOs.TaskManager
{
    public class AddTaskItmDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueTime { get; set; }
        //public bool IsCompleted { get; set; }
        public TaskPriority Priority { get; set; }
   

        //public string UserId { get; set; }
    }
}
