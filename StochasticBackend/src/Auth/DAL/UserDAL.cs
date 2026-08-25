using Microsoft.EntityFrameworkCore;
using StochasticBackend.src.Auth.DTO;
using StochasticBackend.src.Auth.Entities;
using StochasticBackend.src.Shared.DatabasePSQL;

namespace StochasticBackend.src.Auth.DAL
{
    public class UserDAL(ApplicationContext dbContext): IUserDAL
    {
        private readonly ApplicationContext _dbContext = dbContext;

        public async Task<User?> GetUserByLogin(string userLogin)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync((user) => user.Login == userLogin);
            return user;
        }

        public async Task<int> CreateUser(User user)
        {
            try
            {
                _dbContext.Add(user);
                return await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Unable to create user with login {user.Login}", ex);
            }
        }
    }
}
