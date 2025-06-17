using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskManager.DBContext;
using TaskManager.DTOs.Auth;
using TaskManager.Helper;
using TaskManager.InterfaceService;
using TaskManager.IRepository;
using TaskManager.Models;


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
        public async Task<ActionResult<Response>> RegisterUser(RegisterRequestDTO dto)
        {
            _logger.LogInformation("RegisterUser called with Username: {Username}, TenantId: {TenantId}", dto.Username, dto.TenantId);

            try
            {
                var response = await _authService.RegisterUserAsync(dto);
                _logger.LogInformation("User registration completed for Username: {Username} with ResponseCode: {ResponseCode}", dto.Username, response.ResponseCode);
                return StatusCode(HttpStatusMapper.GetHttpStatusCode(response.ResponseCode), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user registration for Username: {Username}", dto.Username);

   
                var errorResponse = ResponseHelper.ServerError();
                return StatusCode(HttpStatusMapper.GetHttpStatusCode(errorResponse.ResponseCode), errorResponse);
            }
        }


        [HttpPost("login")]
        public async Task<ActionResult<Response>> LoginUser(LoginRequestDTO dto)
        {
            _logger.LogInformation("LoginUser called with Username: {Username}", dto.Username);
            try
            {
                var response = await _authService.LoginUserAsync(dto);
                _logger.LogInformation("User login attempt for Username: {Username} with ResponseCode: {ResponseCode}", dto.Username, response.ResponseCode);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user login for Username: {Username}", dto.Username);
                var errorResponse = ResponseHelper.ServerError();
                return StatusCode(HttpStatusMapper.GetHttpStatusCode(errorResponse.ResponseCode), errorResponse);
            }
        }
    }

}
