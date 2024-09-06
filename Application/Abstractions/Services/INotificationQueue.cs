using MediatR;

namespace Application.Abstractions.Services;

public interface INotificationQueue
{
    void Enqueue(INotification notification);
    INotification Dequeue();
    bool HasNotifications { get; }
}