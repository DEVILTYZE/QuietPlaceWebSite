using Microsoft.EntityFrameworkCore;

namespace QuietPlaceWebProject.Interfaces
{
    public interface IImageBoardContext
    {
        DbSet<IBoard> Boards { get; set; }
        DbSet<IThread> Threads { get; set; }
        DbSet<IPost> Posts { get; set; }
        DbSet<IUser> Users { get; set; }
        DbSet<IRole> Roles { get; set; }
        DbSet<IPasscode> Passcodes { get; set; }
        DbSet<IMediaFile> MediaFiles { get; set; }
    }
}