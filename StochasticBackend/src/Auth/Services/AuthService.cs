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
            var user = await _userDAL.GetUserByLogin(userLogin);
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

        public async Task<UserDTO?> RegisterUser(string login, string password)
        {
            Role? role = await _roleDAL.GetRoleByName(UserRoles.REGULAR);
            if (role is null) { return null; }

            User user = new() { Login = login, Password = password, Role = role };

            var result = await _userDAL.CreateUser(user);

            return null;
        }
    }
}
