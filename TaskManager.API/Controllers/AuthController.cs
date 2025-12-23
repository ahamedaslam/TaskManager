using Microsoft.AspNetCore.Mvc;
using TaskManager.DTOs.Auth;
using TaskManager.Helper;
using TaskManager.InterfaceService;
using TaskManager.Models.Response;
using TaskManager.MultiTenant.DTOs;
using TaskManager.Services.Interfaces;

//testing
namespace TaskManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IRefreshTokenService _refreshTokenService;


        //You are not creating objects, you are receiving already-created objects from the DI container through the constructor.

        public AuthController(IAuthService authService, ILogger<AuthController> logger, IRefreshTokenService refreshTokenService)
        {
            _authService = authService;
            _logger = logger;
            _refreshTokenService = refreshTokenService;
        }


        [HttpPost("register")]
        public async Task<ActionResult<Response>> RegisterUser(RegisterRequestDTO dto)
        {
            var logId = Guid.NewGuid().ToString();
            _logger.LogInformation("[{logId}] Registration attempt with Username: {Username}", logId, dto.Username);

            if (dto == null || string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            {
                _logger.LogWarning("Invalid request received for registering new user");
                return BadRequest(ResponseHelper.BadRequest("Invalid registration data."));
            }
            _logger.LogDebug("[{logId}] Entering RegisterUserAsync with Username: {Username}", logId, dto.Username);
            var response = await _authService.RegisterUserAsync(dto, logId);
            _logger.LogInformation("[{logId}] Registration Successfull for Username: {Username}", logId, dto.Username);
            return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
        }




        [HttpPost("login")]
        public async Task<ActionResult> LoginUser(LoginRequestDTO dto)
        {
            var logId = Guid.NewGuid().ToString();
            _logger.LogInformation("[{logId}] Login attempt with Username: {Username}", logId, dto.Username);

            if (dto == null || string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            {
                _logger.LogWarning("[{logId}] Invalid login request: {Request}", logId, dto);
                return BadRequest(ResponseHelper.BadRequest("Invalid login data."));
            }
            _logger.LogDebug("[{logId}] Entering LoginUserAsync with Username: {Username}", logId, dto.Username);
            var response = await _authService.LoginUserAsync(dto, logId);
            _logger.LogInformation("[{logId}] Login Successfull for Username: {Username} ", logId, dto.Username);
            return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);

        }


        [HttpPost("verify-otp")]
        public async Task<ActionResult> VerifyOtp([FromBody] VerifyOtpRequestDTO dto)
        {
            var logId = Guid.NewGuid().ToString();
            _logger.LogInformation("[{logId}] OTP verification attempt for User: {Username}", logId, dto.UserName);
            if (string.IsNullOrWhiteSpace(dto.UserName) || string.IsNullOrWhiteSpace(dto.OTP))
            {
                _logger.LogWarning("[{logId}] OTP-Validation failed due to missing data: {DTO}", logId, dto);
                return BadRequest(ResponseHelper.BadRequest("Username and OTP are required."));
            }
            _logger.LogInformation("[{logId}] OTP verification attempt for user: {Username}", logId, dto.UserName);
            var response = await _authService.VerifyOtpAsync(dto, logId);
            _logger.LogInformation("[{logId}] OTP-Validation Successfull for Username: {Username}", logId, dto.UserName);
            return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
        }


        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var logId = Guid.NewGuid().ToString();
            _logger.LogInformation("[{logId}] Creating refresh token for User",logId);
            if (string.IsNullOrWhiteSpace(request.AccessToken) || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(ResponseHelper.BadRequest("Access token and refresh token are required."));
            }
            var response = await _refreshTokenService.RefreshAsync(request.AccessToken, request.RefreshToken,logId);
            _logger.LogInformation("[{logId}] refresh token created Successfully", logId);
            return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);

        }

        [HttpPost("logout")]
        public async Task<ActionResult<Response>> Logout([FromBody] LogoutRequestDTO dto)
        {
            var logId = Guid.NewGuid().ToString();
            _logger.LogInformation("[{logId}] Logout attempt received", logId);
            if (!ModelState.IsValid)
                return BadRequest(ResponseHelper.NotFound("Token not found"));
            var respone = await _authService.LogoutAsync(dto.RefreshToken,logId);
            _logger.LogInformation("[{logId}] Logout successful", logId);
            return StatusCode(HttpStatusMapper.GetHttpStatusCode(respone.ResponseCode), respone);

        }
    }

}
