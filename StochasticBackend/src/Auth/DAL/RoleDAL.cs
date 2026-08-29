using Microsoft.EntityFrameworkCore;
using StochasticBackend.src.Auth.Entities;
using StochasticBackend.src.Shared.DatabasePSQL;

namespace StochasticBackend.src.Auth.DAL
{
    public class RoleDAL(ApplicationContext dbContext): IRoleDAL
    {
        private readonly ApplicationContext _dbContext = dbContext;

        public async Task<Role?> GetRoleByNameAsync(string name)
        {
            var role = await _dbContext.Roles.FirstOrDefaultAsync(role => role.Name == name);
            return role;
        }
    }
}
