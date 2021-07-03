using Microsoft.EntityFrameworkCore;
using QuietPlaceWebProject.Models;

namespace QuietPlaceWebProject.Interfaces
{
    public interface IUserContext
    {
        DbSet<User> Users { get; set; }
        DbSet<Role> Roles { get; set; }
    }
}