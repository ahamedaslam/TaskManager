using Org.BouncyCastle.Asn1.Ocsp;
using TaskManager.DTOs.Auth;
using TaskManager.Models;
using TaskManager.Models.Response;

namespace TaskManager.InterfaceService
{
    public interface IAuthService
    {
        Task<Response> RegisterUserAsync(RegisterRequestDTO registerRequestDTO,string logId);
        Task<Response> LoginUserAsync(LoginRequestDTO loginRequestDTO,string logId);
    }
}
    