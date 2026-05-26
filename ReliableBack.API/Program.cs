using ReliableBack.API.Extensions;
using ReliableBack.Application;
using ReliableBack.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, _, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddControllers();

    builder.Services
        .AddEndpointsApiExplorer()
        .AddSwaggerGen();

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration)
        .AddApiInfrastructure(builder.Configuration)
        .AddObservability(builder.Configuration);

    var app = builder.Build();

    app.UseApiMiddleware();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors("Dashboard");
    app.UseAuthorization();
    app.UseApiEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}