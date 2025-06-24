using AutoMapper;
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
        private readonly IMapper _mapper; // Assuming you have a mapper for DTO to Entity conversion
        private readonly CurrentUserService _currentUserService;

        public TaskManagerService(ITaskManagerRepo repo, ILogger<TaskManagerService> logger,IMapper mapper, CurrentUserService currentUserService)
        {
            _repo = repo;
            _logger = logger;
            _mapper = mapper; // Initialize the mapper if you are using AutoMapper for DTO to Entity conversion
            _currentUserService = currentUserService; // Assuming you have a service to get current user details

        }

        // var tenantId = _currentUserService.GetTenantId;

        public async Task<Response> GetAllTasksAsync(UserIdDto request,string? filterOn, string? filterQuery, string? sortBy, bool isAscending, int pageNumber, int pageSize,string logId)
        {

            //var (tenantId, role, userId) = UserContextHelper.GetContext(_currentUserService);

            var tenantId = _currentUserService.GetTenantId;
            _logger.LogInformation("[GetAllTasksAsync] Started for logId: {logId}", logId);
            try
            {
                IEnumerable<TaskItem> tasks;

                if (_currentUserService.GetRole == "Admin")
                {
                    _logger.LogInformation("[GetAllTasksAsync] Admin - TenantId: {TenantId}", tenantId);
                    tasks = await _repo.GetAllTasksByTenantIdAsync(tenantId, filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize);
                }

                else if (_currentUserService.GetRole == "Normal")
                {
                    _logger.LogInformation("[GetAllTasksAsync] Normal User - UserId: {UserId}", request.UserId);
                    tasks = await _repo.GetAllTasksAsync(request.UserId, tenantId, filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize);
                }
                else
                {
                    _logger.LogWarning("[GetAllTasksAsync] Unauthorized access. Role: {Role}, UserId: {UserId}, logId: {logId}", _currentUserService.GetRole, request.UserId, logId);
                    return ResponseHelper.Unauthorized("Unauthorized access.");
                }


                if (tasks == null || !tasks.Any())
                {
                    _logger.LogWarning("[GetAllTasksAsync] No tasks found for UserId: {UserId}", request.UserId);
                    return ResponseHelper.NotFound("No Tasks found.");
                }


                _logger.LogInformation("[GetAllTasksAsync] Successfully fetched {Count} tasks for UserId: {UserId}", tasks.Count(), request.UserId);
                return ResponseHelper.Success(logId,tasks,"Tasks Fetched Successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetAllTasksAsync] Error occurred while fetching tasks for UserId: {reqId}", logId);
                return ResponseHelper.ServerError();
            }
        }

        public async Task<Response> GetTaskByIdAsync(TaskDto request,string logId)
        {

            var tenantId = _currentUserService.GetTenantId;
            _logger.LogInformation("[GetTaskByIdAsync] Started - TaskId: {TaskId}, UserId: {UserId}, logId: {logId}", request.taskId, request.userId, logId);
            try
            {
                var task = await _repo.GetTaskByIdAsync(request.taskId, request.userId,tenantId);

                if (task == null)
                {
                    _logger.LogWarning("[GetTaskByIdAsync] Task not found - TaskId: {TaskId}, logId: {logId}", request.taskId, logId);
                    return ResponseHelper.NotFound("Task not found.");
                }

                _logger.LogInformation("[GetTaskByIdAsync] Task retrieved - TaskId: {TaskId}, logId: {logId}", request.taskId, logId);
                return ResponseHelper.Success("Task retrieved.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetTaskByIdAsync] Error occurred while retrieving task with ID: {reqId}", logId);
                return ResponseHelper.ServerError();
            }
        }

        public async Task<Response> CreateTaskAsync(AddTaskItmDTO request, string logId)
        {

            var tenantId = _currentUserService.GetTenantId;
            _logger.LogInformation("[CreateTaskAsync] Creating new task for UserId: {UserId}", request.UserId);

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

                _logger.LogInformation("[CreateTaskAsync] Task created for UserId: {UserId}, logId: {logId}", request.UserId, logId);
                return ResponseHelper.Success(logId,createdTask, "Task created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CreateTaskAsync] Error occurred for logId: {logId}", logId);
                return ResponseHelper.ServerError();
            }
        }



        public async Task<Response> UpdateTaskAsync(TaskItemDTO request,string logId)
        {

            var tenantId = _currentUserService.GetTenantId;
            _logger.LogInformation("[UpdateTaskAsync] Updating task with ID: {TaskId}", logId);
            try
            {
                var result = await _repo.UpdateTaskAsync(request, tenantId);

                if (result == null)
                {
                    _logger.LogWarning("[UpdateTaskAsync] Task not found with ID: {logId}", logId);
                    return ResponseHelper.NotFound("Task not found.");
                }

                _logger.LogInformation("[UpdateTaskAsync] Task updated successfully. TaskId: {TaskId}, logId: {logId}", request.Id, logId);
                return ResponseHelper.Success(logId, result, "Task updated successfully.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateTaskAsync] Error occurred while updating task with requestId: {TaskId}",logId);
                return ResponseHelper.ServerError();
            }
        }

        public async Task<Response> DeleteTaskAsync(TaskDto request,string logId)
        {

            var tenantId = _currentUserService.GetTenantId;
            _logger.LogInformation("[DeleteTaskAsync] Deleting TaskId: {TaskId}, logId: {logId}", request.taskId, logId);
            try
            {
                var isDeleted = await _repo.DeleteTaskAsync(request.taskId, request.userId,tenantId);

                if (!isDeleted)
                {
                    _logger.LogWarning("[DeleteTaskAsync] Task not found - TaskId: {TaskId}, logId: {logId}", request.taskId, logId);
                    return ResponseHelper.NotFound("Task not found.");
                }

                _logger.LogInformation("[DeleteTaskAsync] Deleted TaskId: {TaskId}, logId: {logId}", request.taskId, logId);
                return ResponseHelper.Success("Task deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteTaskAsync] Error occurred while deleting task with ID: {TaskId}", request.taskId);
                return ResponseHelper.ServerError();
            }
        }

        public async Task<Response> SetTaskCompletionStatusAsync(Guid taskId, string userId, bool isCompleted,string logId)
        {

            var tenantId = _currentUserService.GetTenantId;
            _logger.LogInformation("[SetTaskCompletionStatusAsync] Started - TaskId: {TaskId}, IsCompleted: {IsCompleted}, logId: {logId}", taskId, isCompleted, logId);
            try
            {
                var result = isCompleted
                    ? await _repo.MarkTaskAsCompletedAsync(taskId, userId, tenantId)
                    : await _repo.MarkTaskAsNotCompletedAsync(taskId, userId, tenantId);

                if (!result)
                {
                    _logger.LogWarning("[SetTaskCompletionStatusAsync] Task not found - TaskId: {TaskId}, logId: {logId}", taskId, logId);
                    return ResponseHelper.NotFound("Task not found.");
                }

                _logger.LogInformation("[SetTaskCompletionStatusAsync] Updated completion status - TaskId: {TaskId}, IsCompleted: {IsCompleted}, logId: {logId}", taskId, isCompleted, logId);
                return ResponseHelper.Success("Task status updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SetTaskCompletionStatusAsync] Error occurred while updating task status for TaskId: {TaskId}", taskId);
                return ResponseHelper.ServerError();
            }
        }
    }
}
