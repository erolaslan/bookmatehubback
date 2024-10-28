using Microsoft.EntityFrameworkCore;
using BookMateHub.Models;

namespace BookMateHub.Data
{
    public class BookMateHubDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }

        public BookMateHubDbContext(DbContextOptions<BookMateHubDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<Book>().HasKey(b => b.Id);
        }
    }
}
