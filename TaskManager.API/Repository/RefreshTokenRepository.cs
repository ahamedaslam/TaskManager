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

        public RefreshTokenRepository(AuthDBContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken> GenerateAsync(ApplicationUser user)
        {
            var refreshToken = new RefreshToken
            {
                Token = GenerateSecureToken(),
                UserId = user.Id,
                Expires = DateTime.Now.AddDays(7),
                CreatedAt = DateTime.Now
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
            token.Revoked = DateTime.Now;
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
