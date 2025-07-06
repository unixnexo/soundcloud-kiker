using Microsoft.EntityFrameworkCore;
using soundcloud_kiker.Models;

namespace soundcloud_kiker.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Track> Tracks { get; set; }
    }
}
