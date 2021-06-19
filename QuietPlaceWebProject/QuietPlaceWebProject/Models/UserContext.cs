using Microsoft.EntityFrameworkCore;
using QuietPlaceWebProject.Interfaces;

namespace QuietPlaceWebProject.Models
{
    public sealed class UserContext : DbContext, IUserContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Passcode> Passcodes { get; set; }

        public UserContext(DbContextOptions<UserContext> options) : base(options) 
            => Database.EnsureCreated();
    }
}