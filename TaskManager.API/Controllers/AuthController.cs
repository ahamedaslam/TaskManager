using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskManager.DBContext;
using TaskManager.DTOs.Auth;
using TaskManager.Helper;
using TaskManager.InterfaceService;
using TaskManager.IRepository;
using TaskManager.Models.Response;


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
                var response = await _authService.RegisterUserAsync(dto,logId);
                _logger.LogInformation("[{logId}] User registration completed for Username: {Username} with ResponseCode: {ResponseCode}",logId, dto.Username, response.ResponseCode);
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
                var response = await _authService.LoginUserAsync(dto,logId);
                _logger.LogInformation("[{logId}] Login Successfull for Username: {Username} with ResponseCode: {ResponseCode}",logId, dto.Username, response.ResponseCode);
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
