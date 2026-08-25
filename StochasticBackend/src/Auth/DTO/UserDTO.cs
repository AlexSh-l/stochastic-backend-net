using StochasticBackend.src.Auth.Entities;

namespace StochasticBackend.src.Auth.DTO
{
    public record UserDTO
    {
        public Guid Id;
        public required string Login;
        public required Role Role;
    }
}
