// AuthService.cs
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.DBContext;
using TaskManager.DTOs.Auth;
using TaskManager.Helper;
using TaskManager.Interface;
using TaskManager.InterfaceService;
using TaskManager.IRepository;
using TaskManager.Models;
using TaskManager.Models.Response;
using TaskManager.Services.Interfaces;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenRepository _tokenRepository;
    private readonly ILogger<AuthService> _logger;
    private readonly AuthDBContext _context;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AuthService(UserManager<ApplicationUser> userManager,ITokenRepository tokenRepository,ILogger<AuthService> logger,AuthDBContext context, IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration,IEmailService emailService)
    {
        _userManager = userManager;
        _tokenRepository = tokenRepository;
        _logger = logger;
        _context = context;
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
        _emailService = emailService;

    }

    public async Task<Response> RegisterUserAsync(RegisterRequestDTO registerRequestDTO,string logId)
    {
        _logger.LogInformation("[{logId}] Initiating registration for User - {Username}", logId,registerRequestDTO.Username);

        try
        {
            // Validate TenantId
            var tenantId = registerRequestDTO.TenantId;

            var tenantExists = await _context.Tenants.AnyAsync(t => t.Id == tenantId);
            if (!tenantExists)
            {
               return ResponseHelper.BadRequest("Invalid TenantId provided.");
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
                _logger.LogInformation("Started to send welcome email for user {email}", registerRequestDTO.Username);
                
                await _emailService.SendEmailAsync(registerRequestDTO.Username,"Welcome to Task Manager!",$"Hi {registerRequestDTO.Username}, your account has been created successfully!");

                // _logger.LogInformation("[{logId}] User {Username} registered successfully with TenantId: {TenantId}", logId, registerRequestDTO.Username, tenantId);
                return ResponseHelper.Success("User registered successfully..!!");
            }
            else
            {
               // var errorDetails = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                _logger.LogWarning("[{logId}] User registration failed for {Username} with errors: {Errors}", logId, registerRequestDTO.Username, string.Join(", ", identityResult.Errors.Select(e => e.Description)));
                return ResponseHelper.BadRequest("User registration failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{logId}] Error during user registration", logId);

           return ResponseHelper.ServerError();
        }
    }



    public async Task<Response> LoginUserAsync(LoginRequestDTO req, string logId)
    {
        _logger.LogInformation("[{logId}] Starting login for user {Username}", logId, req.Username);

        try
        {
            var identityUser = await _userManager.FindByEmailAsync(req.Username);
            if (identityUser == null)
                return ResponseHelper.NotFound("User not found");

            var isPasswordValid = await _userManager.CheckPasswordAsync(identityUser, req.Password);
            if (!isPasswordValid)
                return ResponseHelper.Unauthorized("Invalid password");

            // Generate OTP
            var otp = new Random().Next(100000, 999999).ToString();
            identityUser.OTP = otp;
            identityUser.OTPExpiry = DateTime.UtcNow.AddMinutes(5);
            await _userManager.UpdateAsync(identityUser);

            // Send OTP to email
            await _emailService.SendEmailAsync(req.Username, "Your OTP Code", $"Your OTP is: {otp}");
            _logger.LogInformation("[{logId}] OTP sent to email: {Email}", logId, req.Username);

            return ResponseHelper.Success("OTP sent to your email. Please verify.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{logId}] Error during login", logId);
            return ResponseHelper.ServerError();
        }
    }


    public async Task<Response> VerifyOtpAsync(VerifyOtpRequestDTO dto, string logId)
    {
        var user = await _userManager.FindByEmailAsync(dto.UserName);
        if (user == null)
            return ResponseHelper.NotFound("User not found");

        if (user.OTP != dto.OTP || user.OTPExpiry == null || user.OTPExpiry < DateTime.UtcNow)
            return ResponseHelper.BadRequest("Invalid or expired OTP.");

        // Clear OTP
        user.OTP = null;
        user.OTPExpiry = null;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("[{logId}] OTP verified successfully for {Username}", logId, dto.UserName);

        // Call authenticate logic
        var roles = await _userManager.GetRolesAsync(user);
        var jwtToken = _tokenRepository.CreateJwtToken(user, roles.ToList());
        var expiryMinutes = int.Parse(_configuration["JWT_ACCESS_TOKEN_EXPIRY_MINUTES"]);
        var expiry = DateTime.Now.AddMinutes(expiryMinutes);
        var refreshToken = await _refreshTokenRepository.GenerateAsync(user);

        return new Response
        {
            ResponseCode = 0,
            ResponseDescription = "Authentication successful.",
            ResponseDatas = new
            {
                AccessToken = jwtToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = expiry,
                User = new
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Roles = roles,
                    TenantId = user.TenantId
                }
            }
        };
    }



}
