using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.DTOs.TaskManager;
using TaskManager.Helper;
using TaskManager.Models.Response;
using TaskManager.Services;

[ApiController]
[Route("api/[controller]")]
public class TaskManagerController : ControllerBase
{
    private readonly TaskManagerService _taskManagerService;
    private readonly ILogger<TaskManagerController> _logger;

    public TaskManagerController(TaskManagerService taskManagerService, ILogger<TaskManagerController> logger)
    {
        _taskManagerService = taskManagerService;
        _logger = logger;
    }

    [Authorize(Roles = "Admin,Normal")]
    [HttpPost("getTasks")]
    public async Task<ActionResult<Response>> GetAllTasks(UserIdDto request, [FromQuery] string? filterOn,
        [FromQuery] string? filterQuery, [FromQuery] string? sortBy, [FromQuery] bool? isAscending,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        string logId = Guid.NewGuid().ToString();
        _logger.LogInformation("[GetAllTasks] RequestId: {logId}", logId);

        try
        {
            var response = await _taskManagerService.GetAllTasksAsync(request, filterOn, filterQuery, sortBy, isAscending ?? true, pageNumber, pageSize, logId);
            return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GetAllTasks] Unexpected error occurred. RequestId: {logId}", logId);
            return StatusCode(500, ResponseHelper.ServerError($"Request failed. Log ID: {logId}"));
        }
    }

    [Authorize(Roles = "Admin,Normal")]
    [HttpPost("getById")]
    public async Task<ActionResult<Response>> GetTaskById(TaskDto request)
    {
        string logId = Guid.NewGuid().ToString();
        _logger.LogInformation("[GetTaskById] RequestId: {logId}", logId);

        try
        {
            if (request == null || request.taskId == Guid.Empty)
            {
                _logger.LogWarning("[{logId}] Null or invalid request.", logId);
                return BadRequest(ResponseHelper.BadRequest("Invalid request."));
            }
            _logger.LogDebug("Entering GetTaskById with TaskId: {TaskId}, UserId: {UserId}", request.taskId, request.userId);
            var response = await _taskManagerService.GetTaskByIdAsync(request, logId);
            return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GetTaskById] Unexpected error occurred. RequestId: {logId}", logId);
            return StatusCode(500, ResponseHelper.ServerError($"Request failed. Log ID: {logId}"));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("create")]
    public async Task<ActionResult<Response>> CreateTask(AddTaskItmDTO request)
    {
        string logId = Guid.NewGuid().ToString();
        _logger.LogInformation("[CreateTask] RequestId: {logId} | UserId: {UserId} | Title: {Title}", logId, request.UserId, request.Title);

        try
        {
            if (request == null)
            {
                _logger.LogWarning("[{logId}] Null request.", logId);
                return BadRequest(ResponseHelper.BadRequest("Invalid request."));
            }
            _logger.LogDebug("Entering CreateTask with UserId: {UserId}, Title: {Title}", request.UserId, request.Title);
            var response = await _taskManagerService.CreateTaskAsync(request, logId);
            return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CreateTask] Unexpected error occurred. RequestId: {logId}", logId);
            return StatusCode(500, ResponseHelper.ServerError($"Request failed. Log ID: {logId}"));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete")]
    public async Task<ActionResult<Response>> DeleteTask(TaskDto request)
    {
        string logId = Guid.NewGuid().ToString();
        _logger.LogInformation("[DeleteTask] RequestId: {logId} | TaskId: {TaskId}, UserId: {UserId}", logId, request.taskId, request.userId);

        try
        {
            if (request == null || request.taskId == Guid.Empty)
            {
                _logger.LogWarning("[{logId}] Null or invalid request.", logId);
                return BadRequest(ResponseHelper.BadRequest("Invalid request."));
            }
            _logger.LogDebug("Entering DeleteTask with TaskId: {TaskId}, UserId: {UserId}", request.taskId, request.userId);
            var response = await _taskManagerService.DeleteTaskAsync(request,logId);
            return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DeleteTask] Unexpected error occurred. RequestId: {logId}", logId);
            return StatusCode(500, ResponseHelper.ServerError($"Request failed. Log ID: {logId}"));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("update")]
    public async Task<ActionResult<Response>> UpdateTask(TaskItemDTO request)
    {
        string logId = Guid.NewGuid().ToString();
        _logger.LogInformation("[UpdateTask] RequestId: {logId} | TaskId: {TaskId}", logId, request.Id);

        try
        {
            if (request == null)
            {
                _logger.LogWarning("[UpdateTask] Null request. RequestId: {logId}", logId);
                return BadRequest(ResponseHelper.BadRequest("Invalid request."));
            }
            _logger.LogDebug("Entering UpdateTask with TaskId: {TaskId}", request.Id);
            var response = await _taskManagerService.UpdateTaskAsync(request, logId);
            return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UpdateTask] Unexpected error occurred. RequestId: {logId}", logId);
            return StatusCode(500, ResponseHelper.ServerError($"Request failed. Log ID: {logId}"));
        }
    }

    [Authorize(Roles = "Admin,Normal")]
    [HttpPatch("setTaskCompletionStatus")]
    public async Task<ActionResult<Response>> SetTaskCompletionStatus(Guid taskId, string userId, [FromQuery] bool isCompleted)
    {
        string logId = Guid.NewGuid().ToString();
        _logger.LogInformation("[SetTaskCompletionStatus] RequestId: {logId} | TaskId: {TaskId}, IsCompleted: {IsCompleted}", logId, taskId, isCompleted);

        try
        {
            if (taskId == Guid.Empty || string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("[SetTaskCompletionStatus] Null or invalid request. RequestId: {logId}", logId);
                return BadRequest(ResponseHelper.BadRequest("Invalid request."));
            }
            _logger.LogDebug("Entering SetTaskCompletionStatus with TaskId: {TaskId}, UserId: {UserId}, IsCompleted: {IsCompleted}", taskId, userId, isCompleted);
            var response = await _taskManagerService.SetTaskCompletionStatusAsync(taskId, userId, isCompleted,logId);
            return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SetTaskCompletionStatus] Unexpected error occurred. RequestId: {logId}", logId);
            return StatusCode(500, ResponseHelper.ServerError($"Request failed. Log ID: {logId}"));
        }
    }
}
