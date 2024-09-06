using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.ConfigEntity;
using Application.Data;
using Application.Data.OnlineData;
using Domain.Items;
using Domain.Players;
using Newtonsoft.Json;
using Protocols;
using SharedKernel;

namespace Application.Players.Create;

internal sealed class CreatePlayerCommandHandler
    : ICommandHandler<CreatePlayerCommand, PlayerCreateResponseProtocol>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IItemRepository _itemRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IServiceApi _api;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePlayerCommandHandler(IDateTimeProvider dateTimeProvider, IItemRepository itemRepository, IPlayerRepository playerRepository, IServiceApi api, IUnitOfWork unitOfWork)
    {
        _dateTimeProvider = dateTimeProvider;
        _itemRepository = itemRepository;
        _playerRepository = playerRepository;
        _api = api;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PlayerCreateResponseProtocol>> Handle(CreatePlayerCommand command, CancellationToken cancellationToken)
    {
        var response = new PlayerCreateResponseProtocol();
        var playerId = command.PlayerId;
        var player = OnlineCacheController.Instance.GetPlayerOnlineCacheByPlayerId(playerId);
        if (player != null)
        {
            response.ErrorType = ErrorType.Failure;
            return Result.Failure(PlayerErrors.Existed(playerId), response);
        }

        var record = await _playerRepository.GetByIdAsync(playerId);
        if (record != null)
        {
            response.ErrorType = ErrorType.Failure;
            return Result.Failure(PlayerErrors.Existed(playerId), response);
        }

        Result<Email> emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            response.ErrorType = emailResult.Error.Type;
            return Result.Failure(emailResult.Error, response);
        }

        Email email = emailResult.Value;
        if (!await _playerRepository.IsEmailUniqueAsync(email))
        {
            response.ErrorType = emailResult.Error.Type;
            return Result.Failure(PlayerErrors.EmailNotUnique, response);
        }

        var nameResult = Name.Create(command.Name);
        if (nameResult.IsFailure)
        {
            response.ErrorType = nameResult.Error.Type;
            return Result.Failure(nameResult.Error, response);
        }

        Name name = nameResult.Value;
        if (!await _playerRepository.IsNameUniqueAsync(name))
        {
            response.ErrorType = nameResult.Error.Type;
            return Result.Failure(PlayerErrors.NameNotUnique, response);
        }

        record = Player.Create(playerId, email, name, _dateTimeProvider.UtcNow);
        _playerRepository.Insert(record);

        player = new PlayerOnlineCache(record, _dateTimeProvider, _playerRepository);
        OnlineCacheController.Instance.RegisterPlayer(player);
        player.Login(false);

        var cfg = _api.GetSingleData<player_init_cfg>(1);
        if (cfg != null
        && !string.IsNullOrEmpty(cfg.items))
        {
            var itemMap = new Dictionary<Guid, Item>();
            OnlineCacheController.Instance.RegisterItemMap(playerId, itemMap);
            var items = JsonConvert.DeserializeAnonymousType(cfg.items, new[]
            {
                new { id = 0, num = 0 }
            });

            foreach (var o in items)
            {
                var item = Item.Create(o.id, o.num, playerId);
                _itemRepository.Insert(item);
                itemMap.Add(item.Id, item);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        response.ErrorType = ErrorType.None;

        return response;
    }
}