using StochasticBackend.src.Auth.Configuration;
using StochasticBackend.src.Auth.DAL;
using StochasticBackend.src.Auth.DTO;
using StochasticBackend.src.Auth.Entities;
using StochasticBackend.src.Shared.DatabasePSQL;

namespace StochasticBackend.src.Auth.Services
{
    public class AuthService(IUserDAL userDAL, IRoleDAL roleDAL) : IAuthService
    {
        private readonly IUserDAL _userDAL = userDAL;
        private readonly IRoleDAL _roleDAL = roleDAL;

        public async Task<UserDTO?> LogUserIn(string userLogin)
        {
            var user = await _userDAL.GetUserByLoginAsync(userLogin);
            if (user is null)
            {
                return null;
            }

            UserDTO? userDTO = null;
            if (user is not null)
            {
                userDTO = new UserDTO
                {
                    Id = user.Id,
                    Login = user.Login,
                    Role = user.Role,
                };
            }

            return userDTO;
        }

        public async Task<bool> RegisterUserAsync(string login, string password)
        {
            Role? role = await _roleDAL.GetRoleByNameAsync(UserRoles.REGULAR);
            if (role is null) { return false; }

            User user = new() { Login = login, Password = password, Role = role };

            try
            {
                var result = await _userDAL.CreateUserAsync(user);
                return true;
            }
            catch (DatabaseException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
