using Microsoft.EntityFrameworkCore;
using ReliableBack.Domain.Tasks;

namespace ReliableBack.Infrastructure.Persistence;

public class ReliableBackDbContext : DbContext
{
    public ReliableBackDbContext(DbContextOptions<ReliableBackDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReliableBackDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}