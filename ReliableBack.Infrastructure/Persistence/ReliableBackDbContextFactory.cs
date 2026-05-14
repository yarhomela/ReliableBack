using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ReliableBack.Infrastructure.Persistence;

public class ReliableBackDbContextFactory : IDesignTimeDbContextFactory<ReliableBackDbContext>
{
    public ReliableBackDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ReliableBackDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=reliableback;Username=postgres;Password=postgres");

        return new ReliableBackDbContext(optionsBuilder.Options);
    }
}