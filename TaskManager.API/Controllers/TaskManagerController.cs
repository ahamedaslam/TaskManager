using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.DTOs.TaskManager;
using TaskManager.Helper;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Controllers
{

    [ApiController] 
    [Route("api/[controller]")]

    //•	If your application supports multiple authentication schemes (e.g., cookies, API keys, Bearer tokens), this attribute restricts access to only Bearer-authenticated users.
    public class TaskManagerController : ControllerBase
    {
        private readonly TaskManagerService _taskManagerService;
        private readonly ILogger<TaskManagerController> _logger;
        //Role-Based Access Control (RBAC)

        public TaskManagerController(TaskManagerService taskManagerService, ILogger<TaskManagerController> logger)
        {
            _taskManagerService = taskManagerService;
            _logger = logger;
        }

        [Authorize(Roles = "Admin,Normal")]
        [HttpPost("getTasks")]
        public async Task<ActionResult<Response>> GetAllTasks(UserIdDto request, string tenantId,[FromQuery] string? filterOn,
            [FromQuery] string? filterQuery, [FromQuery] string? sortBy, [FromQuery] bool? isAscending,
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("[Controller] GetAllTasks called with UserId: {UserId}", request.UserId);

            var response = await _taskManagerService.GetAllTasksAsync(request,tenantId, filterOn, filterQuery, sortBy, isAscending ?? true, pageNumber, pageSize);

            return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
        }

        [Authorize(Roles = "Admin,Normal")]
        [HttpPost("getById")]
        public async Task<ActionResult<Response>> GetTaskById(TaskDto request)
        {
            _logger.LogInformation("[Controller] GetTaskById called with TaskId: {TaskId}, UserId: {UserId}", request.taskId, request.userId);

            var response = await _taskManagerService.GetTaskByIdAsync(request,request.tenantId);

            return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<ActionResult<Response>> CreateTask(TaskItemDTO request)
        {
            _logger.LogInformation("The role is: {Role}", User.IsInRole("ADMIN") ? "ADMIN" : "NORMAL");
            _logger.LogInformation("[Controller] CreateTask called for UserId: {UserId}, Title: {Title}", request.UserId, request.Title);

            var response = await _taskManagerService.CreateTaskAsync(request,request.TenantId);

            return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete")]
        public async Task<ActionResult<Response>> DeleteTask(TaskDto request)
        {
            _logger.LogInformation("[Controller] DeleteTask called for TaskId: {TaskId}, UserId: {UserId}", request.taskId, request.userId);

            var response = await _taskManagerService.DeleteTaskAsync(request,request.tenantId);

            return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update")]
        public async Task<ActionResult<Response>> UpdateTask(TaskItemDTO request)
        {
            _logger.LogInformation("[Controller] UpdateTask called for TaskId: {TaskId}", request.Id);

            if (request == null)
            {
                _logger.LogWarning("[Controller] Task update failed: null request");
                return BadRequest(new Response { ResponseCode = 400, ResponseDescription = "Invalid request." });
            }

            var response = await _taskManagerService.UpdateTaskAsync(request, request.TenantId);

            return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
        }

        [Authorize(Roles = "Admin,Normal")]
        [HttpPatch("setTaskCompletionStatus")]
        public async Task<ActionResult<Response>> SetTaskCompletionStatus(Guid taskId, string userId, [FromQuery] bool isCompleted, string tenantId)
        {
            _logger.LogInformation("[Controller] SetTaskCompletionStatus called for TaskId: {TaskId}, IsCompleted: {IsCompleted}", taskId, isCompleted);

            var response = await _taskManagerService.SetTaskCompletionStatusAsync(taskId, userId, isCompleted,tenantId);

            return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
        }


    }
}
