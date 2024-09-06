using Application;
using Application.Abstractions.Services;
using Infrastructure;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Network;
using Serilog;
using SharedKernel;
using SimpleServer;
using System.Reflection;

Console.WriteLine("Hello, CQRS!");

Log.Logger = new LoggerConfiguration().CreateLogger();

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, loggerConfig) =>
        loggerConfig.ReadFrom.Configuration(context.Configuration))
    .ConfigureServices((context, services) =>
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
                .AddSingleton<IConfigService, ExcelManager>()
                .AddSingleton<INetworkService, NetworkManager>()
                .AddSingleton<ITimerService, TimerService>()
                .AddSingleton<IServiceApi, ServiceApi>()
                .AddSingleton<NotificationDispatcher>()
                .AddApplication()
                .AddInfrastructure(context.Configuration)
                .AddHostedService<StartServer>()
                .BuildServiceProvider();
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationWriteDbContext>();

    if (dbContext.Database.GetPendingMigrations().Any())
    {
        Log.Logger.Information("Applying migrations...");
        dbContext.Database.Migrate();
        Log.Logger.Information("Migrations applied.");
    }
    else
    {
        Log.Logger.Information("No pending migrations.");
    }
}

host.Run();