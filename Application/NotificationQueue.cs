using Application.Abstractions.Services;
using MediatR;
using System.Collections.Concurrent;

namespace Application;

public sealed class NotificationQueue : INotificationQueue
{
    private readonly ConcurrentQueue<INotification> _queue = new ConcurrentQueue<INotification>();

    public void Enqueue(INotification notification)
    {
        _queue.Enqueue(notification);
    }

    public INotification Dequeue()
    {
        _queue.TryDequeue(out var notification);
        return notification;
    }

    public bool HasNotifications => !_queue.IsEmpty;
}