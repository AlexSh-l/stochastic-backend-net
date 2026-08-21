using StochasticBackend.src.Auth.Configuration;

namespace StochasticBackend.src.Auth.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = UserRoles.GUEST;

        public User? User { get; set; }

        public List<Permission> Permissions { get; set; } = new();
    }
}
