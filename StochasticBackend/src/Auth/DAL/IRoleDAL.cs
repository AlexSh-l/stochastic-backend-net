using StochasticBackend.src.Auth.Entities;
using StochasticBackend.src.Shared.Services;

namespace StochasticBackend.src.Auth.DAL
{
    public interface IRoleDAL: IScopedService
    {
        Task<Role?> GetRoleByNameAsync(string name);
    }
}
