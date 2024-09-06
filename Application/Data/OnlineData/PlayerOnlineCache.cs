using Domain.Players;
using Application.Abstractions.Services;

namespace Application.Data.OnlineData;

public class PlayerOnlineCache
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPlayerRepository _playerRepository;
    private Player _record;

    public PlayerOnlineCache(Player player, IDateTimeProvider dateTimeProvider, IPlayerRepository playerRepository)
    {
        _record = player;
        _dateTimeProvider = dateTimeProvider;
        _playerRepository = playerRepository;
    }

    public Player Record { get => _record; }

    public Guid Id { get => _record.Id; }

    public string Email { get => _record.Email.Value; }
    public bool IsOnline { get; private set; }
    public long LoginTimestamp { get => _record.LoginTimestamp; }
    public long LogOutTimestamp { get => _record.LogOutTimestamp; }
    public string Name { get => _record.Name.Value; }

    public void Login(bool isUpdate = true)
    {
        IsOnline = true;
        _record.LoginTimestamp = _dateTimeProvider.UtcNow.Ticks;
        if (isUpdate)
            _playerRepository.Update(_record, nameof(_record.LoginTimestamp));
    }

    public void LogOut()
    {
        IsOnline = false;
        _record.LogOutTimestamp = _dateTimeProvider.UtcNow.Ticks;
        _playerRepository.Update(_record, nameof(_record.LogOutTimestamp));
    }
}