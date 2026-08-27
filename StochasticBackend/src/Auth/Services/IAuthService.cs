using StochasticBackend.src.Auth.DTO;
using StochasticBackend.src.Shared.Services;

namespace StochasticBackend.src.Auth.Services
{
    public interface IAuthService
    {
        Task<UserDTO?> LogUserInAsync(string userLogin, string userPassword);
        Task<UserDTO?> RegisterUserAsync(string login, string password);
    }
}
