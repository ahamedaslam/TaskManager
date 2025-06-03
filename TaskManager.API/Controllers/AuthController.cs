using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TaskManager.DTOs.Auth;
using TaskManager.IRepository;
using TaskManager.Models;


namespace TaskManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenRepository _tokenRepository;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserManager<IdentityUser> userManager,
                               ITokenRepository tokenRepository,
                               ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _tokenRepository = tokenRepository;
            _logger = logger;
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<Response>> RegisterUser([FromBody] RegisterRequestDTO registerRequestDTO)
        {
            var logId = Guid.NewGuid();

            {
                _logger.LogDebug("RegisterUser invoked with payload: {Payload}", JsonSerializer.Serialize(registerRequestDTO));

                try
                {
                    _logger.LogInformation("Initiating user registration for {Username} - {LogId}", registerRequestDTO.Username, logId);

                    var identityUser = new IdentityUser
                    {
                        UserName = registerRequestDTO.Username,
                        Email = registerRequestDTO.Username
                    };

                    var identityResult = await _userManager.CreateAsync(identityUser, registerRequestDTO.Password);

                    if (identityResult.Succeeded)
                    {
                        if (registerRequestDTO.Roles != null && registerRequestDTO.Roles.Any())
                        {
                            foreach (var role in registerRequestDTO.Roles)
                            {
                                await _userManager.AddToRoleAsync(identityUser, role);
                                _logger.LogDebug("Role '{Role}' assigned to user {Username}", role, registerRequestDTO.Username);
                            }
                        }

                        var response = new Response
                        {
                            ResponseCode = 0,
                            ResponseDescription = "User registered successfully."
                        };

                        _logger.LogDebug("RegisterUser response: {Response}", JsonSerializer.Serialize(response));
                        return Ok(response);
                    }
                    else
                    {
                        var errorDetails = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                        var response = new Response
                        {
                            ResponseCode = 1001,
                            ResponseDescription = "Failed to register user: " + errorDetails
                        };

                        _logger.LogWarning("User registration failed - {LogId}. Errors: {Errors}", logId, errorDetails);
                        _logger.LogDebug("RegisterUser response: {Response}", JsonSerializer.Serialize(response));
                        return Ok(response);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error during registration - {LogId}", logId);

                    var response = new Response
                    {
                        ResponseCode = 3236,
                        ResponseDescription = $"An unexpected error occurred: {ex.Message}"
                    };

                    _logger.LogDebug("RegisterUser response: {Response}", JsonSerializer.Serialize(response));
                    return Ok(response);
                }
            }
        }


        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<Response>> LoginUser([FromBody] LoginRequestDTO req)
        {
            var logId = Guid.NewGuid();


            
                _logger.LogDebug($"LoginUser invoked with username: {req.Username}");

                try
                {
                    _logger.LogInformation("Starting login for user {Username} - {LogId}", req.Username, logId);

                    var identityUser = await _userManager.FindByEmailAsync(req.Username);

                    if (identityUser == null)
                    {
                        var response = new Response
                        {
                            ResponseCode = 1003,
                            ResponseDescription = "User not found."
                        };

                        _logger.LogWarning("Login failed: User not found - {Username}, LogId: {LogId}", req.Username, logId);
                        _logger.LogDebug("LoginUser response: {Response}", JsonSerializer.Serialize(response));
                        return Ok(response);
                    }

                    var isPasswordValid = await _userManager.CheckPasswordAsync(identityUser, req.Password);

                    if (!isPasswordValid)
                    {
                        var response = new Response
                        {
                            ResponseCode = 1002,
                            ResponseDescription = "Invalid password."
                        };

                        _logger.LogWarning("Login failed: Invalid password for user {Username} - {LogId}", req.Username, logId);
                        _logger.LogDebug($"LoginUser response: {Response}", response);
                        return Ok(response);
                    }

                    var roles = await _userManager.GetRolesAsync(identityUser);
                    var jwtToken = _tokenRepository.CreateJwtToken(identityUser, roles.ToList());

                    var tokenExpiryMinutes = 15;
                    var expiryDateTime = DateTime.Now.AddMinutes(tokenExpiryMinutes);

                    var successResponse = new Response
                    {
                        ResponseCode = 0,
                        ResponseDescription = "Login successful.",
                        ResponseDatas = new
                        {
                            Token = jwtToken,
                            ExpiresAt = expiryDateTime,
                            TokenType = "Bearer",
                            User = new
                            {
                                req.Username,
                                Roles = roles
                            }
                        }
                    };

                    _logger.LogInformation("User logged in successfully - {Username}, LogId: {LogId}", req.Username, logId);
                    _logger.LogDebug("LoginUser response: {Response}", JsonSerializer.Serialize(successResponse));
                    return Ok(successResponse);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error during login - {LogId}", logId);

                    var response = new Response
                    {
                        ResponseCode = 3236,
                        ResponseDescription = $"An unexpected error occurred: {ex.Message}"
                    };

                    _logger.LogDebug("LoginUser response: {Response}", response);
                    return Ok(response);
                }
            
        }

    }
}
