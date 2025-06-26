using Microsoft.AspNetCore.Mvc;
using TaskManager.DTOs.Auth;
using TaskManager.Helper;
using TaskManager.InterfaceService;

//testing
namespace TaskManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult> RegisterUser(RegisterRequestDTO dto)
        {
            var logId = Guid.NewGuid().ToString();
            _logger.LogInformation("[{logId}] RegisterUser called with Username: {Username}, TenantId: {TenantId}",logId, dto.Username, dto.TenantId);

            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                {
                    _logger.LogWarning("[{logId}] Invalid registration request: {Request}", logId, dto);
                    return BadRequest(ResponseHelper.BadRequest("Invalid registration data."));
                }
                _logger.LogDebug("[{logId}] Entering RegisterUserAsync with Username: {Username}, TenantId: {TenantId}", logId, dto.Username, dto.TenantId);
                var response = await _authService.RegisterUserAsync(dto,logId);
                //_logger.LogInformation("[{logId}] User registration completed for Username: {Username} with ResponseCode: {ResponseCode}",logId, dto.Username, response.ResponseCode);
                return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{logId}] Error occurred during user registration for Username: {Username}",logId,dto.Username);

   
                var errorResponse = ResponseHelper.ServerError();
                return StatusCode(HttpStatusMapper.GetHttpStatusCode(errorResponse.ResponseCode), errorResponse);
            }
        }


        [HttpPost("login")]
        public async Task<ActionResult> LoginUser(LoginRequestDTO dto)
        {
            var logId = Guid.NewGuid().ToString();  
            _logger.LogInformation("[{logId}] Login attempt with Username: {Username}", logId,dto.Username);
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                {
                    _logger.LogWarning("[{logId}] Invalid login request: {Request}", logId, dto);
                    return BadRequest(ResponseHelper.BadRequest("Invalid login data."));
                }
                _logger.LogDebug("[{logId}] Entering LoginUserAsync with Username: {Username}", logId, dto.Username);
                var response = await _authService.LoginUserAsync(dto,logId);
                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{logId}] Error occurred during user login for Username: {Username}", logId,dto.Username);
                var errorResponse = ResponseHelper.ServerError();
                return StatusCode(HttpStatusMapper.GetHttpStatusCode(errorResponse.ResponseCode), errorResponse);
            }
        }
    }

}
