using StochasticBackend.src.Auth.DTO;
using StochasticBackend.src.Auth.Entities;
using StochasticBackend.src.Shared.Services;

namespace StochasticBackend.src.Auth.DAL
{
    public interface IUserDAL: IScopedService
    {
        Task<User?> GetUserByLoginAsync(string userLogin);
        Task<int> CreateUserAsync(User user);
    }
}
