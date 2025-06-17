using TaskManager.DTOs.Auth;
using TaskManager.Models;

namespace TaskManager.InterfaceService
{
    public interface IAuthService
    {
        Task<Response> RegisterUserAsync(RegisterRequestDTO registerRequestDTO);
        Task<Response> LoginUserAsync(LoginRequestDTO loginRequestDTO);
    }
}
