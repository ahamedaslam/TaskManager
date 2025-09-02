using AutoMapper;
using Microsoft.AspNetCore.Identity.Data;
using TaskManager.DTOs.TaskManager;
using TaskManager.Helper;
using TaskManager.IRepository;
using TaskManager.Models;
using TaskManager.Models.Response;

namespace TaskManager.Services
{
    public class TaskManagerService 
    {
        private readonly ITaskManagerRepo _repo;
        private readonly ILogger<TaskManagerService> _logger;
        private readonly CurrentUserService _currentUserService;

        public TaskManagerService(ITaskManagerRepo repo, ILogger<TaskManagerService> logger , CurrentUserService currentUserService)
        {
            _repo = repo;
            _logger = logger;
            _currentUserService = currentUserService; // Assuming you have a service to get current user details
            //structured logging
            //With structured logging, the log message is not formatted unless needed (e.g., based on log level).
        }

        // var tenantId = _currentUserService.GetTenantId;

        public async Task<Response> GetAllTasksAsync(UserIdDto request,string? filterOn, string? filterQuery, string? sortBy, bool isAscending, int pageNumber, int pageSize,string logId)
        {

            //var (tenantId, role, userId) = UserContextHelper.GetContext(_currentUserService);

            var tenantId = _currentUserService.GetTenantId;
            _logger.LogInformation("[{logId}] Started to retreive All tasks", logId);
            try
            {
                IEnumerable<TaskItem> tasks;

                if (_currentUserService.GetRole == "Admin")
                {
                    _logger.LogInformation("[{logId}] Getting tasks for user admin with TenantId: {TenantId}", logId,tenantId);
                    tasks = await _repo.GetAllTasksByTenantIdAsync(tenantId, filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize);
                }

                else if (_currentUserService.GetRole == "Normal")
                {
                    _logger.LogInformation("[{logId}] Getting tasks for user normal with TenantId: {TenantId}", logId, tenantId);
                    tasks = await _repo.GetAllTasksAsync(request.UserId, tenantId, filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize);
                }
                else
                {
                    _logger.LogWarning("[{logId}] Unauthorized access. Role: {Role}, UserId: {UserId}", logId,_currentUserService.GetRole, request.UserId);
                    return ResponseHelper.Unauthorized("Unauthorized access.");
                }


                if (tasks == null || !tasks.Any())
                {
                    _logger.LogWarning("[{logId}] No tasks found for UserId: {UserId}", logId,request.UserId);
                    return ResponseHelper.NotFound("No Tasks found.");
                }


                _logger.LogInformation("[{logId}] Successfully fetched {Count} tasks for UserId: {UserId}", logId,tasks.Count(), request.UserId);
                return ResponseHelper.Success(tasks,"Tasks Fetched Successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{logId}] Error occurred while fetching tasks with userId {userId}", logId,request.UserId);
                return ResponseHelper.ServerError();
            }
        }

        public async Task<Response> GetTaskByIdAsync(TaskDto request,string logId)
        {

            var tenantId = _currentUserService.GetTenantId;
            _logger.LogInformation("[{logId}] Started - TaskId: {TaskId}, UserId: {UserId}", logId,request.taskId, request.userId);
            try
            {
                var task = await _repo.GetTaskByIdAsync(request.taskId, request.userId,tenantId);

                if (task == null)
                {
                    _logger.LogWarning("[{logId}] Task not found - TaskId: {TaskId} & UserId: {UserId}", logId,request.taskId,request.userId);
                    return ResponseHelper.NotFound("Task not found.");
                }

                _logger.LogInformation("[{logId}] Task retrieved - TaskId: {TaskId} & UserId: {UserId}", logId,request.taskId,request.userId);
                return ResponseHelper.Success("Task retrieved.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{logId}] Error occurred while retrieving - TaskId: {TaskId} & UserId: {UserId}", logId,request.taskId,request.userId);
                return ResponseHelper.ServerError();
            }
        }

        public async Task<Response> CreateTaskAsync(AddTaskItmDTO request, string logId)
        {

            var tenantId = _currentUserService.GetTenantId;
            _logger.LogInformation("[{logId}] Creating new task for UserId: {UserId}", logId,request.UserId);

            if (string.IsNullOrWhiteSpace(request.Title))
                return ResponseHelper.BadRequest("Title is required.");

            if (string.IsNullOrWhiteSpace(request.Description))
                return ResponseHelper.BadRequest("Description is required.");

            if (string.IsNullOrWhiteSpace(request.UserId))
                return ResponseHelper.BadRequest("UserId is required.");

            try
            {
                var taskItem = new TaskItem
                {
                    Title = request.Title,
                    Description = request.Description,
                    UserId = request.UserId,
                    TenantId = tenantId,
                    IsCompleted = false,
                    Priority = request.Priority,
                };

                var createdTask = await _repo.CreateTaskAsync(taskItem, tenantId);

                _logger.LogInformation("[{logId}] Task created for UserId: {UserId}", logId,request.UserId);
                return ResponseHelper.Success(createdTask, "Task created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{logId}] Error occurred while creating tasks - UserId: {UserId}", logId,request.UserId);
                return ResponseHelper.ServerError();
            }
        }



        public async Task<Response> UpdateTaskAsync(TaskItemDTO request,string logId)
        {

            var tenantId = _currentUserService.GetTenantId;
            _logger.LogInformation("[{logId}] Updating task with - TaskId: {TaskId} & UserId: {UserId}", logId,request.Id,request.UserId);
            try
            {
                var result = await _repo.UpdateTaskAsync(request, tenantId);

                if (result == null)
                {
                    _logger.LogWarning("[{logId}] Task not found - TaskId: {TaskId} & UserId: {UserId}", logId, request.Id, request.UserId);
                    return ResponseHelper.NotFound("Task not found.");
                }

                _logger.LogInformation("[{logId}] Task updated successfully - TaskId: {TaskId} & UserId: {UserId}", logId,request.Id,request.UserId);
                return ResponseHelper.Success(result, "Task updated successfully.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{logId}] Error occurred while updating task - TaskId: {TaskId} & UserId: {UserId}", logId, request.Id, request.UserId);
                return ResponseHelper.ServerError();
            }
        }

        public async Task<Response> DeleteTaskAsync(TaskDto request,string logId)
        {

            var tenantId = _currentUserService.GetTenantId;
            _logger.LogInformation("[{logId}] Deleting Tasks -  TaskId: {TaskId} & UserId: {UserId}", logId,request.taskId,request.userId);
            try
            {
                var isDeleted = await _repo.DeleteTaskAsync(request.taskId, request.userId,tenantId);

                if (!isDeleted)
                {

                    _logger.LogWarning("[{logId}] Task not found - TaskId: {TaskId} & UserId: {UserId}", logId,request.taskId,request.userId);
                    return ResponseHelper.NotFound("Task not found.");
                }

                _logger.LogInformation("[{logId}] Deleted TaskId: {TaskId}, UserId: {UserId}", logId,request.taskId, request.userId);
                return ResponseHelper.Success("Task deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{logId}] Error occurred while deleting task with ID: {TaskId}", logId,request.taskId);
                return ResponseHelper.ServerError();
            }
        }

        public async Task<Response> SetTaskCompletionStatusAsync(Guid taskId, string userId, bool isCompleted,string logId)
        {

            var tenantId = _currentUserService.GetTenantId;
            _logger.LogInformation("[{logId}] Status update Started - TaskId: {TaskId} & UserId: {UserId} & IsCompleted: {IsCompleted}", logId,taskId, userId,isCompleted);
            try
            {
                var result = isCompleted
                    ? await _repo.MarkTaskAsCompletedAsync(taskId, userId, tenantId)
                    : await _repo.MarkTaskAsNotCompletedAsync(taskId, userId, tenantId);

                if (!result)
                {
                    _logger.LogWarning("[{logId}] Task not found - TaskId: {TaskId} & UserId: {UserId}", logId,taskId,userId);
                    return ResponseHelper.NotFound("Task not found.");
                }

                _logger.LogInformation("[{logId}] Updated completion status - TaskId: {TaskId} & UserId: {UserId} & IsCompleted: {IsCompleted}", logId,taskId,userId,isCompleted);
                return ResponseHelper.Success("Task status updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{logId}] Error occurred while updating task status for TaskId: {TaskId}", logId,taskId);
                return ResponseHelper.ServerError();
            }
        }
    }
}
