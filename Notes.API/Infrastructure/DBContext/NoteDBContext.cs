using Microsoft.EntityFrameworkCore;
using NotesApp.API.Infrastructure.Models;

namespace NotesApp.API.Infrastructure.DBContext
{
    public class NoteDBContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<User> User { get; set; } = null!;
        public DbSet<Note> Note { get; set; } = null!;
        public DbSet<Role> Role { get; set; } = null!;
        public DbSet<UserRole> UserRole { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<User>(u =>
            {
                _ = u.HasKey(u => u.Id);
                _ = u.HasQueryFilter(u => !u.IsDeleted);
                _ = u.HasIndex(u => u.Email).IsUnique();
                _ = u.HasIndex(u => u.CreatedAt);
                _ = u.HasMany(u => u.Notes);
                _ = u.HasMany(u => u.UserRoles)
                    .WithOne(ur => ur.User)
                    .HasForeignKey(ur => ur.UserId);
            });

            _ = modelBuilder.Entity<Note>(n =>
            {
                _ = n.HasKey(n => n.Id);
                _ = n.HasQueryFilter(n => !n.IsDeleted);
                _ = n.HasIndex(n => n.UserId);
                _ = n.HasIndex(n => new { n.UserId, n.UpdatedAt });
                _ = n.HasOne(n => n.User);
            });

            _ = modelBuilder.Entity<Role>(r =>
            {
                _ = r.HasKey(r => r.Id);
                _ = r.HasIndex(r => r.Name).IsUnique();
                _ = r.HasMany(r => r.UserRoles)
                    .WithOne(ur => ur.Role)
                    .HasForeignKey(ur => ur.RoleId);
            });

            _ = modelBuilder.Entity<UserRole>(ur =>
            {
                _ = ur.HasKey(ur => ur.Id);
                _ = ur.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
                _ = ur.HasIndex(ur => ur.UserId);
                _ = ur.HasIndex(ur => ur.RoleId);
            });
        }
    }
}