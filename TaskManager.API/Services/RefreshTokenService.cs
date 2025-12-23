using Microsoft.AspNetCore.Identity;
using TaskManager.Interface;
using TaskManager.IRepository;
using TaskManager.Models;
using TaskManager.Models.Response;
using TaskManager.Services.Interfaces;
using TaskManager.Helper;

namespace TaskManager.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly ILogger<RefreshTokenService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenRepository _tokenRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _configuration;
        private readonly CurrentUserService _currentUserService;
        private readonly RedisService _redisService;


        public RefreshTokenService(ILogger<RefreshTokenService> logger,UserManager<ApplicationUser> userManager, ITokenRepository tokenRepository, IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration, CurrentUserService currentUserService, RedisService redisService)
        {
            _logger = logger;
            _userManager = userManager;
            _tokenRepository = tokenRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
            _currentUserService = currentUserService;
            _redisService = redisService;
        }
        public async Task<Response> GenerateTokensAsync(ApplicationUser user)
        {
            _logger.LogInformation("Generating tokens for UserId: {UserId}, Email: {Email}", user.Id, user.Email);

            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation("Retrieved roles: {Roles} for UserId: {UserId}", string.Join(", ", roles), user.Id);

            var accessToken = _tokenRepository.CreateJwtToken(user, roles.ToList());
            _logger.LogInformation("Access token generated for UserId: {UserId}", user.Id);

            var refreshToken = await _refreshTokenRepository.GenerateAsync(user);
            _logger.LogInformation("Refresh token generated with ID: {TokenId} and Expiry: {Expiry}", refreshToken.TokenId, refreshToken.Expires);

            return new Response
            {
                ResponseCode = 0,
                ResponseDescription = "Refresh Token generated successfully",
                ResponseDatas = new
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken.Token
                }
            };
        }


        public async Task<Response> RefreshAsync(string accessToken, string refreshToken, string logId)
        {
            try
            {
                var principal = JwtTokenUtils.GetPrincipalFromExpiredToken(accessToken, _configuration);
                // Even if access token is expired, still allow me to read claims from it.
                if (principal == null)
                {
                  return ResponseHelper.BadRequest("Invalid access token");
                }
                var storedUserId = await _redisService.GetRefreshTokenAsync(refreshToken);
                if (storedUserId == null)
                    return ResponseHelper.Unauthorized("Invalid refresh token");


                var userId = _currentUserService.GetUserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return ResponseHelper.BadRequest("User ID not found in token");
                }

                var tokenFromDb = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
                if (tokenFromDb == null || tokenFromDb.UserId != userId || tokenFromDb.Expires < DateTime.UtcNow || tokenFromDb.Revoked != null)
                {
                 return ResponseHelper.BadRequest("Invalid or expired refresh token");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                   return ResponseHelper.NotFound("User not found");
                }

                await _refreshTokenRepository.InvalidateAsync(tokenFromDb);
                var tokensResponse = await GenerateTokensAsync(user);
                return tokensResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{logId}] Unexpected error during creating refresh token", logId);
                throw;
            }
        }
    }
}
