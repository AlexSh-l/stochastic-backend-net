using StochasticBackend.src.Auth.DTO;
using StochasticBackend.src.Shared.Services;

namespace StochasticBackend.src.Auth.Services
{
    public interface IAuthService
    {
        Task<UserDTO?> LogUserIn(string userLogin, string userPassword);
        Task<bool> RegisterUserAsync(string login, string password);
    }
}
