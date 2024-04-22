using ConcertFinder.Models;
using Microsoft.EntityFrameworkCore;

namespace ConcertFinder.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        public DbSet<User> Users { get; set; }
    }
}
