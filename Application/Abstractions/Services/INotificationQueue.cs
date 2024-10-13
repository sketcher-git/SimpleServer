using MediatR;

namespace Application.Abstractions.Services;

public interface INotificationQueue
{
    Task Enqueue(INotification notification);
    INotification Dequeue();
    bool HasNotifications { get; }
}