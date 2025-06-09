using Microsoft.EntityFrameworkCore;
using System;
using TaskManager.DBContext;
using TaskManager.DTOs.TaskManager;
using TaskManager.IRepository;
using TaskManager.Models;

namespace TaskManager.Repository
{
    public class TaskManagerRepo : ITaskManagerRepo
    {
        private readonly AuthDBContext _appDbContext; // Inject DI
        private readonly ILogger<TaskManagerRepo> _logger; // Inject logger

        public TaskManagerRepo(AuthDBContext appDbContext, ILogger<TaskManagerRepo> logger)
        {
            _appDbContext = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<TaskItem> CreateTaskAsync(TaskItem taskItem)
        {
            await _appDbContext.TaskItems.AddAsync(taskItem);
            await _appDbContext.SaveChangesAsync();
            return taskItem;
        }

        public async Task<bool> DeleteTaskAsync(Guid taskId, string userId)
        {
            //FindAsync is best when you want to retrieve an entity by its primary key.
            var taskItem = await _appDbContext.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
            if (taskItem == null)
            {
                return false; // Task not found
            }
            _appDbContext.TaskItems.Remove(taskItem);
            await _appDbContext.SaveChangesAsync();
            return true; // Task deleted successfully
        }

        //IEnumerable<T>

        //In C#, IEnumerable<T> and List<T> are both used to represent collections,
        //Supports deferred execution: Especially useful with LINQ and Entity Framework, where the query isn't executed until you actually enumerate the results.
        //Read-only: You can only read (enumerate) items, not add or remove them.

        //List<T> is a concrete collection type that allows for dynamic resizing and provides methods for adding, removing, and accessing elements by index.
        // that stores items in memory.
        public async Task<IEnumerable<TaskItem>> GetAllTasksAsync(string userId, string? filterOn = null, string? filterQuery = null, string? sortBy = null, bool Ascending = true, int pageNumber = 1, int pageSize = 10)
        {
         // Start with base query
            var tasksQuery = _appDbContext.TaskItems.AsQueryable();

            // Apply filtering
            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                if (filterOn.Equals("Title", StringComparison.OrdinalIgnoreCase))
                {
                    tasksQuery = tasksQuery.Where(x => x.Title.Contains(filterQuery));
                }
            }

            // Apply sorting
            // Example: sortBy = "Priority", Ascending = false
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy.Equals("Title", StringComparison.OrdinalIgnoreCase))
                {
                  //•	If Ascending is true, it sorts the tasks in ascending (A-Z) order by title.
                  //•	If Ascending is false, it sorts in descending(Z - A) order.
                  //.   Ternary operator
                    tasksQuery = Ascending ? tasksQuery.OrderBy(x => x.Title) : tasksQuery.OrderByDescending(x => x.Title);
                }
                else if (sortBy.Equals("Priority", StringComparison.OrdinalIgnoreCase))
                {
                    tasksQuery = Ascending ? tasksQuery.OrderBy(x => x.Priority) : tasksQuery.OrderByDescending(x => x.Priority);
                }
                else if (sortBy.Equals("DueTime", StringComparison.OrdinalIgnoreCase))
                {
                    tasksQuery = Ascending ? tasksQuery.OrderBy(x => x.DueTime) : tasksQuery.OrderByDescending(x => x.DueTime);
                }
            }

            // Apply pagination
            // Pagination is the process of breaking a large dataset into smaller parts (pages), each containing a fixed number of records
            var skipResults = (pageNumber - 1) * pageSize;  // (3 - 1) * 10 = 20 recs...so it skips the first 20 records and starts from the 21st.

            return await tasksQuery.Skip(skipResults).Take(pageSize).ToListAsync();

        }

        public async Task<TaskItem> GetTaskByIdAsync(Guid taskId, string userId)
        {
            return await _appDbContext.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
        }

        public async Task<bool> MarkTaskAsCompletedAsync(Guid taskId, string userId)
        {
            var taskItem = await _appDbContext.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
            if (taskItem == null)
            {
                return false; // Task not found
            }
            if (taskItem.IsCompleted)
            {
                return true; // Already completed
            }
            taskItem.IsCompleted = true;
            _appDbContext.TaskItems.Update(taskItem);
            await _appDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkTaskAsNotCompletedAsync(Guid taskId, string userId)
        {
            var taskItem = await _appDbContext.TaskItems
                .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
            if (taskItem == null)
            {
                return false; // Task not found
            }
            if (!taskItem.IsCompleted)
            {
                return true; // Already not completed
            }
            taskItem.IsCompleted = false;
            _appDbContext.TaskItems.Update(taskItem);
            await _appDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<TaskItem> UpdateTaskAsync(TaskItemDTO taskItem)
        {
            var existingTask = await _appDbContext.TaskItems.FirstOrDefaultAsync(t => t.Id == taskItem.Id && t.UserId == taskItem.UserId);
            if (existingTask == null)
            {
                return null;
            }

            existingTask.Title = taskItem.Title;
            existingTask.Description = taskItem.Description;
            existingTask.DueTime = taskItem.DueTime;
            existingTask.IsCompleted = taskItem.IsCompleted;
            existingTask.Priority = taskItem.Priority;

            _appDbContext.TaskItems.Update(existingTask);
            await _appDbContext.SaveChangesAsync();
            return existingTask;
        }
    }
}
