using Application.Abstractions.Services;
using MediatR;
using Protocols;
using SharedKernel;

namespace Application.Timer;

public class HeartbeatNotificationHandler : INotificationHandler<HeartbeatNotification>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IServiceApi _api;
    public HeartbeatNotificationHandler(IDateTimeProvider dateTimeProvider, IServiceApi api) 
    {
        _dateTimeProvider = dateTimeProvider;
        _api = api;
    }

    public Task Handle(HeartbeatNotification notification, CancellationToken cancellationToken)
    {
#if DEBUG
        _api.WriteLog(LogLevelType.Notice, $"HeartbeatNotification triggered, and it is {notification.TriggerTime} now.");
#endif
        _api.Broadcast(new HeartbeatNotificationProtocol { NowTimestamp = _dateTimeProvider.UtcNow.Ticks });
        return Task.CompletedTask;
    }
}