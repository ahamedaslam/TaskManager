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
        _logger.LogInformation("[UpdateTask] RequestId: {logId} | TaskId: {TaskId}", logId, request?.Id);

        try
        {
            if (request == null)
            {
                _logger.LogWarning("[UpdateTask] Null request. RequestId: {logId}", logId);
                return BadRequest(ResponseHelper.BadRequest("Invalid request."));
            }

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
