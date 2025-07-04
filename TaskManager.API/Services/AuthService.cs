﻿// AuthService.cs
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

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenRepository _tokenRepository;
    private readonly ILogger<AuthService> _logger;
    private readonly AuthDBContext _context;
    private readonly IMapper _mapper; // Assuming you have a mapper for DTO to Entity conversion
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AuthService(UserManager<ApplicationUser> userManager,ITokenRepository tokenRepository,ILogger<AuthService> logger,AuthDBContext context,IMapper mapper,IRefreshTokenRepository refreshTokenRepository)
    {
        _userManager = userManager;
        _tokenRepository = tokenRepository;
        _logger = logger;
        _context = context;
        _mapper = mapper;
        _refreshTokenRepository = refreshTokenRepository;

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

               // _logger.LogInformation("[{logId}] User {Username} registered successfully with TenantId: {TenantId}", logId, registerRequestDTO.Username, tenantId);
                return  ResponseHelper.Success("User registered successfully..!!");
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
            _logger.LogInformation("[{logId}] Searching for user with email {Email}", logId, req.Username);
            var identityUser = await _userManager.FindByEmailAsync(req.Username);

            if (identityUser == null)
            {
                _logger.LogWarning("[{logId}] User not found with email {Email}", logId, req.Username);
                return ResponseHelper.NotFound("User not found");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(identityUser, req.Password);
            if (!isPasswordValid)
            {
                _logger.LogWarning("[{logId}] Invalid password attempt for user {Username}", logId, req.Username);
                return ResponseHelper.Unauthorized("Invalid password");
            }

            var roles = await _userManager.GetRolesAsync(identityUser);
            var jwtToken = _tokenRepository.CreateJwtToken(identityUser, roles.ToList());
            var expiry = DateTime.Now.AddMinutes(15);

            // Generate refresh token
            var refreshToken = await _refreshTokenRepository.GenerateAsync((ApplicationUser)identityUser);
            _logger.LogInformation("[{logId}] Refresh token generated for user {UserId}", logId, identityUser.Id);

            return new Response
            {
                ResponseCode = 0,
                ResponseDescription = "Login successful.",
                ResponseDatas = new
                {
                    AccessToken = jwtToken,
                    RefreshToken = refreshToken.Token,
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
            return ResponseHelper.ServerError();
        }
    }

}
