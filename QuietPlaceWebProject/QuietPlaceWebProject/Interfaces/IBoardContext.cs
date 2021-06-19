using Microsoft.EntityFrameworkCore;
using QuietPlaceWebProject.Models;

namespace QuietPlaceWebProject.Interfaces
{
    public interface IBoardContext
    {
        DbSet<Board> Boards { get; set; }
        DbSet<Thread> Threads { get; set; }
        DbSet<Post> Posts { get; set; }
        DbSet<MediaFile> MediaFiles { get; set; }
    }
}