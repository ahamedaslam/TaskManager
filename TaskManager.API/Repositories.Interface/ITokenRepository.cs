using Microsoft.AspNetCore.Identity;

namespace TaskManager.IRepository
{
    public interface ITokenRepository
    {//IdentityUser (from ASP.NET Core Identity), representing the user for whom the token is being created.
        // IdentityUser - used to manage identity users
        string CreateJwtToken(IdentityUser user, List<string> roles);  //he method will return a string, likely a JWT token
    }
}
