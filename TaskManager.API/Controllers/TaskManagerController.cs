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
    private readonly CurrentUserService _currentUserService;

    public TaskManagerController(TaskManagerService taskManagerService,CurrentUserService currentUserService, ILogger<TaskManagerController> logger)
    {
        _taskManagerService = taskManagerService;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    [Authorize(Roles = "Admin,Normal")]
    [HttpPost("getTasks")]
    public async Task<ActionResult<Response>> GetAllTasks([FromQuery] string? filterOn,[FromQuery] string? filterQuery, [FromQuery] string? sortBy, [FromQuery] bool? isAscending,[FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        string logId = Guid.NewGuid().ToString();
        _logger.LogInformation("[GetAllTasks] RequestId: {logId}", logId);
        var tenantId = _currentUserService.GetTenantId;
        var userId = _currentUserService.GetUserId;
       
        var response = await _taskManagerService.GetAllTasksAsync(tenantId, userId, filterOn, filterQuery, sortBy, isAscending ?? true, pageNumber, pageSize, logId);
        _logger.LogInformation("[{logId}] Tasks retrieved Successfully",logId);
        return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
    }

    
    [Authorize(Roles = "Admin,Normal")]
    [HttpPost("getTask")]
    public async Task<ActionResult<Response>> GetTaskById(TaskDto request)
    {
        string logId = Guid.NewGuid().ToString();
        _logger.LogInformation("[GetTaskById] RequestId: {logId}", logId);
        var userId = _currentUserService.GetUserId;
        var tenantId = _currentUserService.GetTenantId;

        // Validate request and Guid value
        if (request == null || request.taskId == Guid.Empty)
        {
            _logger.LogWarning("[{logId}] Null or invalid request.", logId);
            return BadRequest(ResponseHelper.BadRequest("Invalid request."));
        }
        _logger.LogDebug("Entering GetTaskById with TaskId: {TaskId}, UserId: {UserId}", request.taskId, userId);
        var response = await _taskManagerService.GetTaskByIdAsync(request, userId, tenantId, logId);
        return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);

    }
    

    [Authorize(Roles = "Admin")]
    [HttpPost("create")]
    public async Task<ActionResult<Response>> CreateTask(AddTaskItmDTO request)
    {
        string logId = Guid.NewGuid().ToString();
        var userId = _currentUserService.GetTenantId;
        var tenantId = _currentUserService.GetTenantId;
        _logger.LogInformation("[CreateTask] RequestId: {logId} | UserId: {UserId} | Title: {Title}", logId, userId, request.Title);


        if (request == null)
        {
            _logger.LogWarning("[{logId}] Null request.", logId);
            return BadRequest(ResponseHelper.BadRequest("Invalid request."));
        }
        _logger.LogDebug("Entering CreateTask with UserId: {UserId}, Title: {Title}", userId, request.Title);
        var response = await _taskManagerService.CreateTaskAsync(request, userId, tenantId, logId);
        return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);


    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete")]
    public async Task<ActionResult<Response>> DeleteTask(TaskDto request)
    {
        string logId = Guid.NewGuid().ToString();
        var tenantId = _currentUserService.GetTenantId;
        _logger.LogInformation("[DeleteTask] RequestId: {logId} | TaskId: {TaskId}, UserId: {UserId}", logId, request.taskId, request.userId);

        if (request == null || request.taskId == Guid.Empty)
        {
            _logger.LogWarning("[{logId}] Null or invalid request.", logId);
            return BadRequest(ResponseHelper.BadRequest("Invalid request."));
        }
        _logger.LogDebug("Entering DeleteTask with TaskId: {TaskId}, UserId: {UserId}", request.taskId, request.userId);
        var response = await _taskManagerService.DeleteTaskAsync(request, tenantId, logId);
        return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("update")]
    public async Task<ActionResult<Response>> UpdateTask(TaskItemDTO request)
    {
        string logId = Guid.NewGuid().ToString();
        _logger.LogInformation("[UpdateTask] RequestId: {logId} | TaskId: {TaskId}", logId, request.Id);

        if (request == null)
        {
            _logger.LogWarning("[UpdateTask] Null request. RequestId: {logId}", logId);
            return BadRequest(ResponseHelper.BadRequest("Invalid request."));
        }
        _logger.LogDebug("Entering UpdateTask with TaskId: {TaskId}", request.Id);
        var response = await _taskManagerService.UpdateTaskAsync(request, logId);
        return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
    }

    [Authorize(Roles = "Admin,Normal")]
    [HttpPatch("setTaskCompletionStatus")]
    public async Task<ActionResult<Response>> SetTaskCompletionStatus(Guid taskId, string userId, [FromQuery] bool isCompleted)
    {
        string logId = Guid.NewGuid().ToString();
        var tenantId = _currentUserService.GetTenantId;
        _logger.LogInformation("[SetTaskCompletionStatus] RequestId: {logId} | TaskId: {TaskId}, IsCompleted: {IsCompleted}", logId, taskId, isCompleted);


        if (taskId == Guid.Empty || string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("[SetTaskCompletionStatus] Null or invalid request. RequestId: {logId}", logId);
            return BadRequest(ResponseHelper.BadRequest("Invalid request."));
        }
        _logger.LogDebug("Entering SetTaskCompletionStatus with TaskId: {TaskId}, UserId: {UserId}, IsCompleted: {IsCompleted}", taskId, userId, isCompleted);
        var response = await _taskManagerService.SetTaskCompletionStatusAsync(taskId, userId, tenantId, isCompleted, logId);
        return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);

    }
}
