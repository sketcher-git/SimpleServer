using Application.Abstractions.Behaviors;
using Application.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            config.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(QueryCachingPipelineBehavior<,>));
        });

        services.AddSingleton<IProtocolCommandService, ProtocolCommandService>();
        services.AddSingleton<INotificationQueue, NotificationQueue>();

        return services;
    }
}