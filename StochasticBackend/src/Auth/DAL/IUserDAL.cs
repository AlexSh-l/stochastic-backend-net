using StochasticBackend.src.Auth.DTO;
using StochasticBackend.src.Auth.Entities;

namespace StochasticBackend.src.Auth.DAL
{
    public interface IUserDAL
    {
        Task<User?> GetUserByLogin(string userLogin);
        Task<int> CreateUser(User user);
    }
}
