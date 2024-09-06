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
        if (channel == ChatType.Private)
        {
            var target = OnlineCacheController.Instance.GetPlayerOnlineCacheByPlayerId(targetId);
            if (target == null)
            {
                response.ErrorType = ErrorType.NotFound;
                return Result.Failure(Error.NotFound("Chat.TargetOffline", $"Target player with the Id = '{targetId}' was offline"), response);
            }
        }

        notificationQueue.Enqueue(new ChatNotification(command.PlayerId, command.Channel, targetId, command.Content, dateTimeProvider.UtcNow));
        
        response.ErrorType = ErrorType.None;
        return response;
    }
}