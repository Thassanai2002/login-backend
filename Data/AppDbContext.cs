using LoginApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LoginApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasColumnName("id").HasDefaultValueSql("newid()");
            entity.Property(u => u.Username).HasColumnName("username").HasMaxLength(50).IsRequired();
            entity.Property(u => u.PasswordHash).HasColumnName("passwordHash").HasMaxLength(255).IsRequired();
            entity.Property(u => u.CreatedDate).HasColumnName("createdDate").HasDefaultValueSql("sysutcdatetime()");
            entity.HasIndex(u => u.Username).IsUnique();
        });
    }
}