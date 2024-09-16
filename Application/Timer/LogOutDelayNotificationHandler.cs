using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Application.Data;
using Domain.Players;
using MediatR;
using SharedKernel;

namespace Application.Timer;

public sealed class LogOutDelayNotificationHandler : INotificationHandler<LogOutDelayNotification>
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IServiceApi _api;
    private readonly IUnitOfWork _unitOfWork;

    public LogOutDelayNotificationHandler(IPlayerRepository playerRepository, IServiceApi api, IUnitOfWork unitOfWork)
    {
        _playerRepository = playerRepository;
        _api = api;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(LogOutDelayNotification notification, CancellationToken cancellationToken)
    {
        var player = OnlineCacheController.Instance.GetPlayerOnlineCacheByPlayerId(notification.playerId);
        if (player == null
            || player.IsOnline)
            return;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        OnlineCacheController.Instance.ClearAllCacheByPlayerId(notification.playerId);
        _api.WriteLog(LogLevelType.Notice, $"LogoutDelayNotification triggered, and player {notification.playerId} logged out.");
    }
}