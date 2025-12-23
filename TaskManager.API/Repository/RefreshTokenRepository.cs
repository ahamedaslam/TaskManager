using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using TaskManager.DBContext;
using TaskManager.Interface;
using TaskManager.Models;

namespace TaskManager.Repository
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {

        private readonly AuthDBContext _context;
        private readonly IConfiguration _configuration;

        public RefreshTokenRepository(AuthDBContext authDBContext, IConfiguration configuration)
        {
            _context = authDBContext;
            _configuration = configuration;
        }


        public async Task<RefreshToken> GenerateAsync(ApplicationUser user)
        {
            var expiryDays = int.Parse(_configuration["JWT_REFRESH_TOKEN_EXPIRY_DAYS"]);

            var refreshToken = new RefreshToken
            {
                Token = GenerateSecureToken(),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(expiryDays)
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                        .Include(r => r.User)
                        .FirstOrDefaultAsync(r => r.Token == token);
        }

        public async Task InvalidateAsync(RefreshToken token)
        {
            token.Revoked = DateTime.UtcNow;
            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();
        }

        private static string GenerateSecureToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
