using Application.Abstractions.Services;
using MediatR;
using Protocols;
using SharedKernel;

namespace Application.Players.Login;

internal sealed class LoginNotificationHandler(IServiceApi api)
    : INotificationHandler<LoginNotification>
{
    public Task Handle(LoginNotification notification, CancellationToken cancellationToken)
    {
        var response = new LoginNotificationProtocol
        {
            PlayerId = notification.PlayerId,
            Name = notification.Name
        };

        api.Broadcast(response);
        return Task.CompletedTask;
    }
}