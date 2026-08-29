namespace StochasticBackend.src.Auth.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Role> Roles { get; set; } = new();
    }
}
