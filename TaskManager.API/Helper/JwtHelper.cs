using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TaskManager.Helper
{
    public static class JwtTokenUtils
    {
        // Returns the ClaimsPrincipal from an expired (or valid) JWT token.
        // Parameters:
        // - token: the JWT string to validate and extract claims from.
        // - configuration: IConfiguration instance to read Jwt_Issuer, Jwt_Audience, Jwt_Secret.
        // Returns: ClaimsPrincipal? extracted from the token, or null if token is invalid.
        public static ClaimsPrincipal? GetPrincipalFromExpiredToken(string token, IConfiguration configuration)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false, // allow reading claims from expired tokens
                ValidIssuer = configuration["Jwt_Issuer"],
                ValidAudience = configuration["Jwt_Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt_Secret"] ?? string.Empty)
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                throw;
            }
        }
    }
}