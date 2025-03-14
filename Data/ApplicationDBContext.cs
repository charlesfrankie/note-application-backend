using Microsoft.EntityFrameworkCore;
using NoteApplication.Models.Entities;

namespace NoteApplication.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions options) : base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Note> Note { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Note>()
                .HasOne(n => n.User)  // Note belongs to a User
                .WithMany(u => u.Notes) // User has many Notes
                .HasForeignKey(n => n.UserId)  // Foreign key in Note
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete (optional)
        }
    }
}
