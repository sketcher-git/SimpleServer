using Application.Abstractions.Services;
using MediatR;

namespace SimpleServer;

public sealed class NotificationDispatcher
{
    private readonly IMediator _mediator;
    private readonly INotificationQueue _notificationQueue;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    public NotificationDispatcher(IMediator mediator, INotificationQueue notificationQueue)
    {
        _mediator = mediator;
        _notificationQueue = notificationQueue;
    }

    private async Task DispatchNotifications(CancellationToken token)
    {
        while (true)
        {
            var notification = _notificationQueue.Dequeue();
            if (notification == null)
            {
                await Task.Delay(100, token);
                continue;
            }

            await _mediator.Publish(notification, token);
        }
    }

    public void StartDispatchNotifications()
    {
        Task.Run(() => DispatchNotifications(_cts.Token));
    }
}