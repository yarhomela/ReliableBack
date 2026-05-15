using ReliableBack.Infrastructure;
using ReliableBack.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<TaskWorkerService>();

var host = builder.Build();
host.Run();