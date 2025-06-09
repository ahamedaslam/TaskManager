using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Channels;
using TaskManager.DBContext;
using TaskManager.DTOs.TaskManager;
using TaskManager.IRepository;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskManagerController : ControllerBase
    {
        private readonly ITaskManagerRepo _taskManagerRepo;
        private readonly ILogger<TaskManagerController> _logger;

        public TaskManagerController(ITaskManagerRepo taskManagerRepo, ILogger<TaskManagerController> logger)
        {
            _taskManagerRepo = taskManagerRepo;
            _logger = logger;
        }

        [HttpPost]
        [Route("getTasks")]
        public async Task<ActionResult<Response>> getAllTasks(UserIdDto request)
        {
            _logger.LogInformation("getAllTasks called with UserId: {UserId} at {Time}", request.UserId, DateTime.UtcNow);
            try
            {
                var tasks = await _taskManagerRepo.GetAllTasksAsync(request.UserId);
                if (tasks == null || !tasks.Any())
                {
                    _logger.LogWarning("No tasks found for userId: {UserId} at {Time}", request.UserId, DateTime.UtcNow);
                    return NotFound(new Response { ResponseCode = 404, ResponseDescription = "No tasks found." });
                }
                _logger.LogInformation("Tasks fetched successfully for userId: {UserId} at {Time}. Count: {Count}", request.UserId, DateTime.UtcNow, tasks.Count());
                return Ok(new Response
                {
                    ResponseCode = 0,
                    ResponseDescription = "Tasks fetched successfully.",
                    ResponseDatas = tasks
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tasks for userId: {UserId} at {Time}", request.UserId, DateTime.UtcNow);
                return StatusCode(500, new Response { ResponseCode = 500, ResponseDescription = "Internal server error." });
            }
        }

        [HttpPost]
        [Route("getById")]
        public async Task<ActionResult<Response>> GetTaskById(TaskDto request)
        {
            _logger.LogInformation("GetTaskById called with TaskId: {TaskId}, UserId: {UserId} at {Time}", request.taskId, request.userId, DateTime.UtcNow);
            try
            {
                var task = await _taskManagerRepo.GetTaskByIdAsync(request.taskId, request.userId);
                if (task == null)
                {
                    _logger.LogWarning("Task with ID: {TaskId} not found for userId: {UserId} at {Time}", request.taskId, request.userId, DateTime.UtcNow);
                    return NotFound(new Response { ResponseCode = 404, ResponseDescription = "Task not found." });
                }
                _logger.LogInformation("Task with ID: {TaskId} fetched successfully for userId: {UserId} at {Time}", request.taskId, request.userId, DateTime.UtcNow);
                return Ok(new Response
                {
                    ResponseCode = 0,
                    ResponseDescription = "Task fetched successfully.",
                    ResponseDatas = task
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching task with ID: {TaskId} for userId: {UserId} at {Time}", request.taskId, request.userId, DateTime.UtcNow);
                return StatusCode(500, new Response { ResponseCode = 500, ResponseDescription = "Internal server error." });
            }
        }

        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<Response>> CreateTask(TaskItemDTO request)
        {
            _logger.LogInformation("CreateTask called for userId: {UserId} at {Time} with Title: {Title}", request.UserId, DateTime.UtcNow, request.Title);
            try
            {
                var taskItem = new TaskItem
                {
                    Title = request.Title,
                    Description = request.Description,
                    UserId = request.UserId,
                    IsCompleted = false,
                    Priority = request.Priority,
                };
                var createdTask = await _taskManagerRepo.CreateTaskAsync(taskItem);
                _logger.LogInformation("Task created successfully with ID: {TaskId} for userId: {UserId} at {Time}", createdTask.Id, request.UserId, DateTime.UtcNow);
                return CreatedAtAction(nameof(GetTaskById), new { taskId = createdTask.Id, userId = request.UserId }, new Response
                {
                    ResponseCode = 0,
                    ResponseDescription = "Task created successfully.",
                    ResponseDatas = createdTask
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task for userId: {UserId} at {Time}", request.UserId, DateTime.UtcNow);
                return StatusCode(500, new Response { ResponseCode = 500, ResponseDescription = "Internal server error." });
            }
        }

        [HttpDelete]
        [Route("delete")]
        public async Task<ActionResult<Response>> DeleteTask(TaskDto request)
        {
            _logger.LogInformation("DeleteTask called with TaskId: {TaskId} for userId: {UserId} at {Time}", request.taskId, request.userId, DateTime.UtcNow);
            try
            {
                var isDeleted = await _taskManagerRepo.DeleteTaskAsync(request.taskId, request.userId);
                if (!isDeleted)
                {
                    _logger.LogWarning("Task with ID: {TaskId} not found for userId: {UserId} at {Time}", request.taskId, request.userId, DateTime.UtcNow);
                    return NotFound(new Response { ResponseCode = 404, ResponseDescription = "Task not found." });
                }
                _logger.LogInformation("Task with ID: {TaskId} deleted successfully for userId: {UserId} at {Time}", request.taskId, request.userId, DateTime.UtcNow);
                return Ok(new Response
                {
                    ResponseCode = 0,
                    ResponseDescription = "Task deleted successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task with ID: {TaskId} for userId: {UserId} at {Time}", request.taskId, request.userId, DateTime.UtcNow);
                return StatusCode(500, new Response { ResponseCode = 500, ResponseDescription = "Internal server error." });
            }
        }

        [HttpPut]
        [Route("update")]
        public async Task<ActionResult<Response>> UpdateTask([FromBody] TaskItemDTO request)
        {
            _logger.LogInformation("UpdateTask called for TaskId: {TaskId} at {Time}", request.Id, DateTime.UtcNow);
            if (request == null)
            {
                _logger.LogWarning("Task is null");
                return BadRequest(new Response
                {
                    ResponseCode = 400,
                    ResponseDescription = "Task ID mismatch."
                });
            }
            try
            {
                var result = await _taskManagerRepo.UpdateTaskAsync(request);
                if (result == null)
                {
                    _logger.LogWarning("Task with ID: {TaskId} not found for update at {Time}", request.Id, DateTime.UtcNow);
                    return NotFound(new Response
                    {
                        ResponseCode = 404,
                        ResponseDescription = "Task not found."
                    });
                }

                _logger.LogInformation("Task with ID: {TaskId} updated successfully at {Time}", request.Id, DateTime.UtcNow);
                return Ok(new Response
                {
                    ResponseCode = 0,
                    ResponseDescription = "Task updated successfully.",
                    ResponseDatas = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task with ID: {TaskId} at {Time}", request.Id, DateTime.UtcNow);
                return StatusCode(500, new Response { ResponseCode = 500, ResponseDescription = "Internal server error." });
            }
        }

        [HttpPatch]
        [Route("setTaskCompletionStatus")]
        public async Task<ActionResult<Response>> SetTaskCompletionStatus(Guid taskId, string userId, [FromQuery] bool isCompleted)
        {

            _logger.LogInformation("SetTaskCompletionStatus called for TaskId: {TaskId}, UserId: {UserId}, IsCompleted: {IsCompleted} at {Time}", taskId, userId, isCompleted, DateTime.UtcNow);

            bool result = isCompleted ? await _taskManagerRepo.MarkTaskAsCompletedAsync(taskId, userId) : await _taskManagerRepo.MarkTaskAsNotCompletedAsync(taskId, userId);

            if (!result)
            {
                var notFoundResponse = new Response
                {
                    ResponseCode = 404,
                    ResponseDescription = "Task not found."
                };
                return NotFound(notFoundResponse);
            }

            var successResponse = new Response
            {
                ResponseCode = 0,
                ResponseDescription = "Task completion status updated successfully."
            };
            return Ok(successResponse);
        }

    }
}
