using StochasticBackend.src.Auth.DTO;

namespace StochasticBackend.src.Auth.Services
{
    public interface IAuthService
    {
        Task<UserDTO?> LogUserIn(string userLogin);
        Task<bool> RegisterUserAsync(string login, string password);
    }
}
