using Microsoft.EntityFrameworkCore;
using QuietPlaceWebProject.Interfaces;

namespace QuietPlaceWebProject.Models
{
    public sealed class BoardContext : DbContext, IBoardContext
    {
        public DbSet<Board> Boards { get; set; }
        public DbSet<Thread> Threads { get; set; }
        public DbSet<Post> Posts { get; set; }
        // public DbSet<MediaFile> MediaFiles { get; set; }
        public DbSet<Captcha> Captchas { get; set; }
        
        public BoardContext(DbContextOptions<BoardContext> options) : base(options) 
            => Database.EnsureCreated();
    }
}