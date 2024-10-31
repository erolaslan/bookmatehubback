using Microsoft.EntityFrameworkCore;
using BookMateHub.Models;

namespace BookMateHub.Data
{
    public class BookMateHubDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Note> Notes { get; set; } // Notes DbSet'i eklendi

        public BookMateHubDbContext(DbContextOptions<BookMateHubDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<Book>().HasKey(b => b.Id);

            // Book ve Note arasındaki ilişki: Her kitap birçok not içerebilir
            modelBuilder.Entity<Note>()
                .HasOne(n => n.Book)         // Bir notun bir kitabı vardır
                .WithMany(b => b.Notes)      // Bir kitabın birden fazla notu olabilir
                .HasForeignKey(n => n.BookId) // Note tablosunda BookId yabancı anahtar olarak
                .OnDelete(DeleteBehavior.Cascade); // Kitap silindiğinde ilgili notlar da silinir
        }
    }
}
