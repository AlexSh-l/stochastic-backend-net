using StochasticBackend.src.Auth.DTO;

namespace StochasticBackend.src.Auth.Services
{
    public interface IAuthService
    {
        UserDTO? LogUserIn();
    }
}
