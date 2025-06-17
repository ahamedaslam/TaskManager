// AuthService.cs
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaskManager.DBContext;
using TaskManager.DTOs.Auth;
using TaskManager.InterfaceService;
using TaskManager.IRepository;
using TaskManager.Models;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenRepository _tokenRepository;
    private readonly ILogger<AuthService> _logger;
    private readonly AuthDBContext _context;
    private readonly IMapper _mapper; // Assuming you have a mapper for DTO to Entity conversion

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenRepository tokenRepository,
        ILogger<AuthService> logger,
        AuthDBContext context,IMapper mapper)
    {
        _userManager = userManager;
        _tokenRepository = tokenRepository;
        _logger = logger;
        _context = context;
        _mapper = mapper;

    }

    public async Task<Response> RegisterUserAsync(RegisterRequestDTO registerRequestDTO)
    {
        var logId = Guid.NewGuid();
        _logger.LogInformation("Initiating registration for {Username} - {LogId}", registerRequestDTO.Username, logId);

        try
        {
            // Validate TenantId
            var tenantId = registerRequestDTO.TenantId;

            var tenantExists = await _context.Tenants.AnyAsync(t => t.Id == tenantId);
            if (!tenantExists)
            {
                return new Response
                {
                    ResponseCode = 1002,
                    ResponseDescription = "Invalid TenantId provided."
                };
            }

            // Create User
            var identityUser = new ApplicationUser
            {
                UserName = registerRequestDTO.Username,
                Email = registerRequestDTO.Username,
                TenantId = tenantId
            };

            var identityResult = await _userManager.CreateAsync(identityUser, registerRequestDTO.Password);

            if (identityResult.Succeeded)
            {
                if (registerRequestDTO.Roles?.Any() == true)
                {
                    foreach (var role in registerRequestDTO.Roles)
                    {
                        await _userManager.AddToRoleAsync(identityUser, role);
                        _logger.LogDebug("Assigned role {Role} to user {Username}", role, registerRequestDTO.Username);
                    }
                }

                return new Response
                {
                    ResponseCode = 0,
                    ResponseDescription = "User registered successfully."
                };
            }
            else
            {
                var errorDetails = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                return new Response
                {
                    ResponseCode = 1001,
                    ResponseDescription = "Failed to register user: " + errorDetails
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration - {LogId}", logId);

            return new Response
            {
                ResponseCode = 3236,
                ResponseDescription = $"An unexpected error occurred: {ex.Message}"
            };
        }
    }

    

    public async Task<Response> LoginUserAsync(LoginRequestDTO req)
    {
        var logId = Guid.NewGuid();
        _logger.LogInformation("Starting login for user {Username} - {LogId}", req.Username, logId);

        try
        {
            var identityUser = await _userManager.FindByEmailAsync(req.Username);

            if (identityUser == null)
            {
                return new Response
                {
                    ResponseCode = 1003,
                    ResponseDescription = "User not found."
                };
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(identityUser, req.Password);
            if (!isPasswordValid)
            {
                return new Response
                {
                    ResponseCode = 1002,
                    ResponseDescription = "Invalid password."
                };
            }

            var roles = await _userManager.GetRolesAsync(identityUser);
            var jwtToken = _tokenRepository.CreateJwtToken(identityUser, roles.ToList());

            var expiry = DateTime.Now.AddMinutes(15);

            return new Response
            {
                ResponseCode = 0,
                ResponseDescription = "Login successful.",
                ResponseDatas = new
                {
                    Token = jwtToken,
                    ExpiresAt = expiry,
                    TokenType = "Bearer",
                    User = new
                    {
                        req.Username,
                        Roles = roles,
                        TenantId = identityUser.TenantId
                    }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login - {LogId}", logId);

            return new Response
            {
                ResponseCode = 3236,
                ResponseDescription = $"An unexpected error occurred: {ex.Message}"
            };
        }
    }
}
