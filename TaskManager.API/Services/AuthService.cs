// AuthService.cs
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.DBContext;
using TaskManager.DTOs.Auth;
using TaskManager.Helper;
using TaskManager.InterfaceService;
using TaskManager.IRepository;
using TaskManager.Models;
using TaskManager.Models.Response;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenRepository _tokenRepository;
    private readonly ILogger<AuthService> _logger;
    private readonly AuthDBContext _context;
    private readonly IMapper _mapper; // Assuming you have a mapper for DTO to Entity conversion

    public AuthService(UserManager<ApplicationUser> userManager,ITokenRepository tokenRepository,ILogger<AuthService> logger,AuthDBContext context,IMapper mapper)
    {
        _userManager = userManager;
        _tokenRepository = tokenRepository;
        _logger = logger;
        _context = context;
        _mapper = mapper;

    }

    public async Task<Response> RegisterUserAsync(RegisterRequestDTO registerRequestDTO,string logId)
    {
        _logger.LogInformation("[{logId}] Initiating registration for {Username}", logId,registerRequestDTO.Username);

        try
        {
            // Validate TenantId
            var tenantId = registerRequestDTO.TenantId;

            var tenantExists = await _context.Tenants.AnyAsync(t => t.Id == tenantId);
            if (!tenantExists)
            {
               return ResponseHelper.BadRequest(logId,"Invalid TenantId provided.");
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
                        _logger.LogDebug("[{logId}] Assigned role {Role} to user {Username}", logId,role, registerRequestDTO.Username);
                    }
                }

                return  ResponseHelper.Success(logId,"User registered successfully..!!");
            }
            else
            {
                var errorDetails = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                return ResponseHelper.BadRequest(logId, $"User registration failed: {errorDetails}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{logId}] Error during user registration", logId);

           return ResponseHelper.ServerError(logId);
        }
    }

    

    public async Task<Response> LoginUserAsync(LoginRequestDTO req,string logId)
    {

        _logger.LogInformation("[{logId}] Starting login for user {Username}", logId,req.Username);

        try
        {
            var identityUser = await _userManager.FindByEmailAsync(req.Username);

            if (identityUser == null)
            {
             return ResponseHelper.NotFound(logId,"User not found");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(identityUser, req.Password);
            if (!isPasswordValid)
            {
                return ResponseHelper.Unauthorized("Invalid password");
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
                    User = new
                    {
                        UserId = identityUser.Id,
                        UserName = identityUser.UserName,
                        Roles = roles,
                        TenantId = identityUser.TenantId
                    }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{logId}] Unexpected error during login", logId);

           return ResponseHelper.ServerError(logId);
        }
    }
}
