using Microsoft.EntityFrameworkCore;
using MVVM.Api.Entities;

namespace MVVM.Api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.Property(item => item.Title)
                .HasMaxLength(200)
                .IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}
