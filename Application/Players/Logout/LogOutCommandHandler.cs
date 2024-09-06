using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Data;
using Application.Timer;
using Domain.Players;
using SharedKernel;

namespace Application.Players.Logout;

internal sealed class LogOutCommandHandler : ICommandHandler<LogOutCommand>
{
    private readonly IServiceApi _api;
    private readonly IUnitOfWork _unitOfWork;

    public LogOutCommandHandler(IServiceApi api, IUnitOfWork unitOfWork)
    {
        _api = api;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(LogOutCommand command, CancellationToken cancellationToken)
    {
        var player = OnlineCacheController.Instance.GetPlayerOnlineCacheByPlayerId(command.playerId);
        if (player == null)
            return Result.Failure(PlayerErrors.NotFound(command.playerId));

        player.LogOut();

        _api.CreateTimeEvent(TimeSpan.FromMinutes(1), new LogOutDelayNotification(command.playerId));
        return Result.Success();
    }
}