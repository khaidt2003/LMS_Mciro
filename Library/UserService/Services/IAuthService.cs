using System.Threading.Tasks;
using UserService.DTOs;
using UserService.Models;

namespace UserService.Services
{
    public interface IAuthService
    {
        Task<ServiceResponse<int>> RegisterAsync(UserRegisterDto request);
        Task<ServiceResponse<string>> LoginAsync(UserLoginDto request);
    }
}