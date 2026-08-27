using Microsoft.EntityFrameworkCore;
using StochasticBackend.src.Auth.DTO;
using StochasticBackend.src.Auth.Entities;
using StochasticBackend.src.Shared.DatabasePSQL;

namespace StochasticBackend.src.Auth.DAL
{
    public class UserDAL(ApplicationContext dbContext): IUserDAL
    {
        private readonly ApplicationContext _dbContext = dbContext;

        public async Task<User?> GetUserByLoginAsync(string userLogin)
        {
            try
            {
                var user = await _dbContext.Users.LeftJoin(_dbContext.Roles, 
                    user => user.RoleId, 
                    role => role.Id,
                    (user, role) => new User { 
                        Id = user.Id, 
                        Login = user.Login, 
                        Password = user.Password,
                        RoleId = role != null ? role.Id : 0, 
                        Role = role ?? new Role()
                    }
                ).Where(user => user.Login == userLogin).FirstOrDefaultAsync();
                //var user = await _dbContext.Users.FirstOrDefaultAsync((user) => user.Login == userLogin);
                return user;
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Exception occured while trying to log in user {userLogin}", nameof(User), ex);
            }
        }

        public async Task<int> CreateUserAsync(User user)
        {
            try
            {
                _dbContext.Add(user);
                return await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Unable to create user with login {user.Login}", nameof(User), ex);
            }
        }
    }
}
