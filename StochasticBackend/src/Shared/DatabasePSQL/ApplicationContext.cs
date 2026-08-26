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
            //Database.EnsureDeleted();
            //Database.EnsureCreated();
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=stochastic-psql-db;Username=postgres;Password=St0Ch1Mg{92}");
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ////new User { Login = "TestUser1", Password= "TestPassword1", Role =  }
            //modelBuilder.Entity<Permission>().HasData(
            //    new Permission { Id = 1, Name = "Images:View" },
            //    new Permission { Id = 2, Name = "Images:Edit" }
            //);

            //modelBuilder.Entity<Role>().HasData(
            //    new Role { Id = 1, Name = "Guest" },
            //    new Role { Id = 2, Name = "Regular", },
            //    new Role { Id = 3, Name = "Admin", }
            //);

            //modelBuilder.Entity<User>().HasData()
        }
    }
}
