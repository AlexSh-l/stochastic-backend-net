using StochasticBackend.src.Auth.Configuration;

namespace StochasticBackend.src.Auth.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = UserRoles.GUEST;

        public List<User> Users { get; set; } = new();

        public List<Permission> Permissions { get; set; } = new();
    }
}
