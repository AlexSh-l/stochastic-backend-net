using StochasticBackend.src.Auth.Configuration;
using StochasticBackend.src.Auth.DAL;
using StochasticBackend.src.Auth.DTO;
using StochasticBackend.src.Auth.Entities;
using StochasticBackend.src.Shared.DatabasePSQL;
using StochasticBackend.src.Shared.Services;

namespace StochasticBackend.src.Auth.Services
{
    public class AuthService(IUserDAL userDAL, IRoleDAL roleDAL, IHashingService hashingService) : IAuthService
    {
        private readonly IUserDAL _userDAL = userDAL;
        private readonly IRoleDAL _roleDAL = roleDAL;
        private readonly IHashingService _hashingService = hashingService;

        public async Task<UserDTO?> LogUserInAsync(string userLogin, string userPassword)
        {
            try
            {
                var user = await _userDAL.GetUserByLoginAsync(userLogin);
                if (user is null)
                {
                    return null;
                }

                bool isPasswordValid = await _hashingService.VerifyValueAsync(userPassword, user.Password);

                UserDTO? userDTO = null;
                if (isPasswordValid)
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
            catch (DatabaseException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public async Task<UserDTO?> RegisterUserAsync(string login, string password)
        {
            Role? role = await _roleDAL.GetRoleByNameAsync(UserRoles.REGULAR);
            if (role is null) { return null; }

            string hashedPassword = await _hashingService.HashValueAsync(password);

            User user = new() { Login = login, Password = hashedPassword, Role = role };

            try
            {
                var result = await _userDAL.CreateUserAsync(user);
                UserDTO resultingUser = new() { Id = user.Id, Login = user.Login, Role = user.Role };
                return resultingUser;
            }
            catch (DatabaseException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }
    }
}
