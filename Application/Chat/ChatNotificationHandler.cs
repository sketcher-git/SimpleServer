using Application.Abstractions.Services;
using Application.Data;
using MediatR;
using Protocols;
using SharedKernel;

namespace Application.Chat;

internal sealed class ChatNotificationHandler(IServiceApi api)
    : INotificationHandler<ChatNotification>
{
    public Task Handle(ChatNotification notification, CancellationToken cancellationToken)
    {
        var response = new ChatNotificationProtocol
        {
            SenderId = notification.SenderId,
            Content = notification.Content,
            SendingTime = notification.SendingTime
        };

        if (notification.Channel == ChatType.World)
            api.Broadcast(response);
        else
        {
            var player = OnlineCacheController.Instance.GetPlayerOnlineCacheByPlayerId(notification.TargetId);
            if (player != null)
                api.Send(notification.TargetId, (short)ProtocolId.Chat, response);
        }

        return Task.CompletedTask;
    }
}