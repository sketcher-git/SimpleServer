using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Data;
using Protocols;
using SharedKernel;

namespace Application.Chat;

internal sealed class ChatCommandHandler(IDateTimeProvider dateTimeProvider, INotificationQueue notificationQueue)
    : ICommandHandler<ChatCommand, ChatResponseProtocol>
{
    public async Task<Result<ChatResponseProtocol>> Handle(ChatCommand command, CancellationToken cancellationToken)
    {
        var response = new ChatResponseProtocol();
        var targetId = command.TargetId;
        var channel = command.Channel;
        var senderId = command.PlayerId;
        var sender = OnlineCacheController.Instance.GetPlayerOnlineCacheByPlayerId(senderId);
        if (sender == null)
        {
            response.ErrorType = ErrorType.Failure;
            return Result.Failure(Error.Failure("Chat.SenderOffline", $"Sender with the Id = '{senderId}' was offline"), response);
        }

        if (channel == ChatType.Private)
        {
            var target = OnlineCacheController.Instance.GetPlayerOnlineCacheByPlayerId(targetId);
            if (target == null)
            {
                response.ErrorType = ErrorType.NotFound;
                return Result.Failure(Error.NotFound("Chat.TargetOffline", $"Target player with the Id = '{targetId}' was offline"), response);
            }
        }

        await notificationQueue.Enqueue(new ChatNotification(senderId, command.Channel, targetId, sender.Name, command.Content, dateTimeProvider.UtcNow));
        
        response.ErrorType = ErrorType.None;
        return response;
    }
}