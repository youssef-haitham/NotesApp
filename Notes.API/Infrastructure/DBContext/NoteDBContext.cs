using Microsoft.EntityFrameworkCore;
using NotesApp.API.Infrastructure.Models;

namespace NotesApp.API.Infrastructure.DBContext
{
    public class NoteDBContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<User> User { get; set; } = null!;
        public DbSet<Note> Note { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<User>(u =>
            {
                _ = u.HasKey(u => u.Id);
                _ = u.HasQueryFilter(u => !u.IsDeleted);
                _ = u.HasMany(u => u.Notes);
            });

            _ = modelBuilder.Entity<Note>(n =>
            {
                _ = n.HasKey(n => n.Id);
                _ = n.HasQueryFilter(n => !n.IsDeleted);
                _ = n.HasOne(n => n.User);
            });
        }
    }
}
