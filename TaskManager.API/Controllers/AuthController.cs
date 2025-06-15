using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskManager.DBContext;
using TaskManager.DTOs.Auth;
using TaskManager.IRepository;
using TaskManager.Models;


namespace TaskManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenRepository _tokenRepository;
        private readonly ILogger<AuthController> _logger;
        private readonly AuthDBContext _context; // Assuming you have a DbContext for your application

        public AuthController(UserManager<ApplicationUser> userManager,
                               ITokenRepository tokenRepository,
                               ILogger<AuthController> logger,
                               AuthDBContext context)
        {
            _userManager = userManager;
            _tokenRepository = tokenRepository;
            _logger = logger;
            _context = context;
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<Response>> RegisterUser([FromBody] RegisterRequestDTO registerRequestDTO)
        {
            var logId = Guid.NewGuid();
            _logger.LogDebug("RegisterUser invoked with payload: {Payload}", JsonSerializer.Serialize(registerRequestDTO));

            try
            {
                _logger.LogInformation("Initiating user registration for {Username} - {LogId}", registerRequestDTO.Username, logId);

                string tenantId;

                if (registerRequestDTO.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
                {
                    // Admin - Create new tenant
                    var newTenant = new Tenant
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = $"{registerRequestDTO.Username}_Tenant"
                    };

                    await _context.Tenants.AddAsync(newTenant);
                    await _context.SaveChangesAsync();

                    tenantId = newTenant.Id;
                    _logger.LogInformation("Created new tenant with Id: {TenantId} for admin user", tenantId);
                }
                else
                {
                    // Normal - use provided tenant ID
                    tenantId = registerRequestDTO.TenantId;

                    var tenantExists = await _context.Tenants.AnyAsync(t => t.Id == tenantId);
                    if (!tenantExists)
                    {
                        return BadRequest(new Response
                        {
                            ResponseCode = 1002,
                            ResponseDescription = "Invalid TenantId provided."
                        });
                    }
                }

                var identityUser = new ApplicationUser
                {
                    UserName = registerRequestDTO.Username,
                    Email = registerRequestDTO.Username,
                    TenantId = tenantId
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

                    return Ok(new Response
                    {
                        ResponseCode = 0,
                        ResponseDescription = "User registered successfully."
                    });
                }
                else
                {
                    var errorDetails = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                    return Ok(new Response
                    {
                        ResponseCode = 1001,
                        ResponseDescription = "Failed to register user: " + errorDetails
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration - {LogId}", logId);

                return Ok(new Response
                {
                    ResponseCode = 3236,
                    ResponseDescription = $"An unexpected error occurred: {ex.Message}"
                });
            }
        }


        /// <summary>
        ///     
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        /// aslam@test.com   aslam@123
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
                                Roles = roles,
                                TenantId = identityUser.TenantId,
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
