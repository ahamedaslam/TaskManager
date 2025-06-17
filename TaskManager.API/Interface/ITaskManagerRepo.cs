using TaskManager.DTOs.TaskManager;
using TaskManager.Models;

namespace TaskManager.IRepository
{
    public interface ITaskManagerRepo
    {
        //Filtering, Sorting, Paging
        //Optional parameters like filterOn, filterQuery, and sortBy are nullable,
        
        //Filtering (filterOn, filterQuery)
        //Sorting(sortBy, Ascending)
        //Pagination(pageNumber, pageSize
        Task<IEnumerable<TaskItem>> GetAllTasksAsync(string userId,string tenantid, string? filterOn = null, string? filterQuery = null, string? sortBy = null, bool ascending = true, int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<TaskItem>> GetAllTasksByTenantIdAsync(string tenantid, string? filterOn = null, string? filterQuery = null, string? sortBy = null, bool ascending = true, int pageNumber = 1, int pageSize = 10);
        Task<TaskItem> GetTaskByIdAsync(Guid taskId, string userId, string tenantId);
        Task<TaskItem> CreateTaskAsync(TaskItem taskItem, string tenantId);
        Task<TaskItem> UpdateTaskAsync(TaskItemDTO taskItem, string tenantId);
        Task<bool> DeleteTaskAsync(Guid taskId, string userId, string tenantId);
        Task<bool> MarkTaskAsCompletedAsync(Guid taskId, string userId, string tenantId);
        Task<bool> MarkTaskAsNotCompletedAsync(Guid taskId, string userId, string tenantId);
    }
}
