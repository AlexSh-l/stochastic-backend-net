using StochasticBackend.src.Auth.Entities;

namespace StochasticBackend.src.Auth.DAL
{
    public interface IRoleDAL
    {
        Task<Role?> GetRoleByNameAsync(string name);
    }
}
