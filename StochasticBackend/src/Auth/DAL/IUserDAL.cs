using StochasticBackend.src.Auth.DTO;
using StochasticBackend.src.Auth.Entities;

namespace StochasticBackend.src.Auth.DAL
{
    public interface IUserDAL
    {
        Task<User?> GetUserByLoginAsync(string userLogin);
        Task<int> CreateUserAsync(User user);
    }
}
