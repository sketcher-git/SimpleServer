using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Data;
using Application.Data.OnlineData;
using Application.Players.GetById;
using Domain.Players;
using Protocols;
using SharedKernel;

namespace Application.Players.Login;

internal sealed class LoginCommandHandler
    : ICommandHandler<LoginCommand, LoginResponseProtocol>
{
    private readonly ICacheService _cacheService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPlayerRepository _playerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(ICacheService cacheService, IDateTimeProvider dateTimeProvider, INotificationQueue notificationQueue, IPlayerRepository playerRepository, IUnitOfWork unitOfWork)
    {
        _cacheService = cacheService;
        _dateTimeProvider = dateTimeProvider;
        _playerRepository = playerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginResponseProtocol>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var response = new LoginResponseProtocol();
        var playerId = command.PlayerId;
        var player = OnlineCacheController.Instance.GetPlayerOnlineCacheByPlayerId(playerId);
        if (player == null)
        {
            var record = await _playerRepository.GetByIdAsync(playerId);
            if (record == null)
            {
                response.ErrorType = ErrorType.NotFound;
                return Result.Failure(PlayerErrors.NotFound(playerId), response);
            }

            OnlineCacheController.Instance.RegisterPlayer(player = new PlayerOnlineCache(record, _dateTimeProvider, _playerRepository));
        }

        player.Login();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var query = new GetPlayerByIdQuery(playerId, playerId);
        _cacheService.Remove(query.CacheKey);

        response.ErrorType = ErrorType.None;
        response.LoginTimestamp = player.LoginTimestamp;
        return response;
    }
}