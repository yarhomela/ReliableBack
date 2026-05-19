using Microsoft.EntityFrameworkCore;
using ReliableBack.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace ReliableBack.Tests.Common.Fixtures;

public class PostgreSqlFixture : IAsyncLifetime
{
    public ReliableBackDbContext DbContext { get; private set; } = null!;
    
    public string ConnectionString => _container.GetConnectionString();
    
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("reliableback_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<ReliableBackDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        DbContext = new ReliableBackDbContext(options);
        
        await DbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _container.DisposeAsync();
    }
}