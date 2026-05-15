using ReliableBack.Application;
using ReliableBack.Infrastructure;
using ReliableBack.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<TaskWorkerService>();
builder.Services.AddHostedService<WatchdogService>();

var host = builder.Build();
host.Run();