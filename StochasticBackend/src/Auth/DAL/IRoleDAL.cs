using StochasticBackend.src.Auth.Entities;

namespace StochasticBackend.src.Auth.DAL
{
    public interface IRoleDAL
    {
        Task<Role?> GetRoleByName(string name);
    }
}
