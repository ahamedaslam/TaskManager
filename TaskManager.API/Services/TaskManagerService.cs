//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using TaskManager.DTOs.TaskManager;
//using TaskManager.IRepository;
//using TaskManager.Models;

//namespace TaskManager.Services
//{
//    public class TaskManagerService : ITaskManagerRepo
//    {
//        private readonly ITaskManagerRepo _repo;
//        private readonly ILogger<TaskManagerService> _logger;

//        public TaskManagerService(ITaskManagerRepo repo, ILogger<TaskManagerService> logger)
//        {
//            _repo = repo;
//            _logger = logger;
//        }

//        public async Task<Response> GetAllTasksAsync(UserIdDto request, string? filterOn, string? filterQuery, string? sortBy, bool isAscending, int pageNumber, int pageSize)
//        {
//            _logger.LogInformation("[GetAllTasksAsync] Started fetching tasks for UserId: {UserId}", request.UserId);
//            try
//            {
//                var tasks = await _repo.GetAllTasksAsync(request.UserId, filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize);

//                if (tasks == null || !tasks.Any())
//                {
//                    _logger.LogWarning("[GetAllTasksAsync] No tasks found for UserId: {UserId}", request.UserId);
//                    return new Response { ResponseCode = 404, ResponseDescription = "No tasks found." };
//                }

//                _logger.LogInformation("[GetAllTasksAsync] Successfully fetched {Count} tasks for UserId: {UserId}", tasks.Count(), request.UserId);
//                return new Response { ResponseCode = 0, ResponseDescription = "Tasks fetched successfully.", ResponseDatas = tasks };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "[GetAllTasksAsync] Error occurred while fetching tasks for UserId: {UserId}", request.UserId);
//                return new Response { ResponseCode = 500, ResponseDescription = "Internal server error." };
//            }
//        }

//        public async Task<Response> GetTaskByIdAsync(TaskDto request)
//        {
//            _logger.LogInformation("[GetTaskByIdAsync] Fetching task with ID: {TaskId} for UserId: {UserId}", request.taskId, request.userId);
//            try
//            {
//                var task = await _repo.GetTaskByIdAsync(request.taskId, request.userId);

//                if (task == null)
//                {
//                    _logger.LogWarning("[GetTaskByIdAsync] Task not found with ID: {TaskId} for UserId: {UserId}", request.taskId, request.userId);
//                    return new Response { ResponseCode = 404, ResponseDescription = "Task not found." };
//                }

//                _logger.LogInformation("[GetTaskByIdAsync] Task retrieved successfully with ID: {TaskId}", request.taskId);
//                return new Response { ResponseCode = 0, ResponseDescription = "Task fetched successfully.", ResponseDatas = task };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "[GetTaskByIdAsync] Error occurred while retrieving task with ID: {TaskId}", request.taskId);
//                return new Response { ResponseCode = 500, ResponseDescription = "Internal server error." };
//            }
//        }

//        public async Task<Response> CreateTaskAsync(TaskItemDTO request)
//        {
//            _logger.LogInformation("[CreateTaskAsync] Creating new task for UserId: {UserId}", request.UserId);
//            try
//            {
//                var taskItem = new TaskItem
//                {
//                    Title = request.Title,
//                    Description = request.Description,
//                    UserId = request.UserId,
//                    IsCompleted = false,
//                    Priority = request.Priority,
//                };

//                var createdTask = await _repo.CreateTaskAsync(taskItem);

//                _logger.LogInformation("[CreateTaskAsync] Task created with ID: {TaskId}", createdTask.Id);
//                return new Response { ResponseCode = 0, ResponseDescription = "Task created successfully.", ResponseDatas = createdTask };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "[CreateTaskAsync] Error occurred while creating task for UserId: {UserId}", request.UserId);
//                return new Response { ResponseCode = 500, ResponseDescription = "Internal server error." };
//            }
//        }

//        public async Task<Response> UpdateTaskAsync(TaskItemDTO request)
//        {
//            _logger.LogInformation("[UpdateTaskAsync] Updating task with ID: {TaskId}", request.Id);
//            try
//            {
//                var result = await _repo.UpdateTaskAsync(request);

//                if (result == null)
//                {
//                    _logger.LogWarning("[UpdateTaskAsync] Task not found with ID: {TaskId}", request.Id);
//                    return new Response { ResponseCode = 404, ResponseDescription = "Task not found." };
//                }

//                _logger.LogInformation("[UpdateTaskAsync] Task updated successfully with ID: {TaskId}", request.Id);
//                return new Response { ResponseCode = 0, ResponseDescription = "Task updated successfully.", ResponseDatas = result };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "[UpdateTaskAsync] Error occurred while updating task with ID: {TaskId}", request.Id);
//                return new Response { ResponseCode = 500, ResponseDescription = "Internal server error." };
//            }
//        }

//        public async Task<Response> DeleteTaskAsync(TaskDto request)
//        {
//            _logger.LogInformation("[DeleteTaskAsync] Deleting task with ID: {TaskId} for UserId: {UserId}", request.taskId, request.userId);
//            try
//            {
//                var isDeleted = await _repo.DeleteTaskAsync(request.taskId, request.userId);

//                if (!isDeleted)
//                {
//                    _logger.LogWarning("[DeleteTaskAsync] Task not found with ID: {TaskId}", request.taskId);
//                    return new Response { ResponseCode = 404, ResponseDescription = "Task not found." };
//                }

//                _logger.LogInformation("[DeleteTaskAsync] Task deleted successfully with ID: {TaskId}", request.taskId);
//                return new Response { ResponseCode = 0, ResponseDescription = "Task deleted successfully." };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "[DeleteTaskAsync] Error occurred while deleting task with ID: {TaskId}", request.taskId);
//                return new Response { ResponseCode = 500, ResponseDescription = "Internal server error." };
//            }
//        }

//        public async Task<Response> SetTaskCompletionStatusAsync(Guid taskId, string userId, bool isCompleted)
//        {
//            _logger.LogInformation("[SetTaskCompletionStatusAsync] Updating completion status for TaskId: {TaskId}, IsCompleted: {IsCompleted}", taskId, isCompleted);
//            try
//            {
//                var result = isCompleted
//                    ? await _repo.MarkTaskAsCompletedAsync(taskId, userId)
//                    : await _repo.MarkTaskAsNotCompletedAsync(taskId, userId);

//                if (!result)
//                {
//                    _logger.LogWarning("[SetTaskCompletionStatusAsync] Task not found with ID: {TaskId}", taskId);
//                    return new Response { ResponseCode = 404, ResponseDescription = "Task not found." };
//                }

//                _logger.LogInformation("[SetTaskCompletionStatusAsync] Task status updated successfully for TaskId: {TaskId}", taskId);
//                return new Response { ResponseCode = 0, ResponseDescription = "Task completion status updated successfully." };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "[SetTaskCompletionStatusAsync] Error occurred while updating task status for TaskId: {TaskId}", taskId);
//                return new Response { ResponseCode = 500, ResponseDescription = "Internal server error." };
//            }
//        }
//    }
//}
