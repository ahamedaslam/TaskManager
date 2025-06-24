using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManager.IRepository;
using TaskManager.Models;

namespace TaskManager.Repository
{
    //Purpose: To generate a JWT (JSON Web Token) for a given user, including their email and roles as claims.
    //Claims are pieces of information about the user (like email, roles, user ID, etc.) that are embedded inside the JWT.
    //Claims are used by the application to identify the user and their permissions after the token is issued.
    public class TokenRepository : ITokenRepository
    {
        private readonly IConfiguration _configuration;

        public TokenRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public string CreateJwtToken(IdentityUser user, List<string> roles)
        {
            // Initialize claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            // Add TenantId if the user is of type ApplicationUser
            if (user is ApplicationUser appUser && !string.IsNullOrWhiteSpace(appUser.TenantId))
            {
                claims.Add(new Claim("TenantId", appUser.TenantId));
            }

            // Add each role as a claim
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Retrieve key and credentials from configuration
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Build the token descriptor
            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials
            );

            // Return the serialized JWT token
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

    }
}
