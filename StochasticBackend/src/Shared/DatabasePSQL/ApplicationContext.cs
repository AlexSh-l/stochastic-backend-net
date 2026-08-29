using Microsoft.EntityFrameworkCore;
using StochasticBackend.src.Auth.Entities;

namespace StochasticBackend.src.Shared.DatabasePSQL
{
    public class ApplicationContext: DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;

        public ApplicationContext(DbContextOptions<ApplicationContext> options): base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
