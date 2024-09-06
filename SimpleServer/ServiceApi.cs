using Application.Abstractions.Config;
using Application.Abstractions.Services;
using MersenneTwister;
using Network;
using Serilog;
using SharedKernel;
using SharedKernel.Protocols;

namespace SimpleServer;

public sealed class ServiceApi : IServiceApi
{
    private readonly IConfigService _config;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger _logger;
    private readonly INetworkService _network;
    private readonly ITimerService _timerService;
    private readonly Random _random;

    public ServiceApi(IConfigService config, IDateTimeProvider dateTimeProvider, ILogger logger, INetworkService network, ITimerService timerService)
    {
        _config = config;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
        _network = network;
        _timerService = timerService;
        _random = Randoms.Create(_dateTimeProvider.UtcNow.Millisecond);
    }

    public void Broadcast<T>(T message)
        where T : BaseProtocol, INotificationProtocol, new()
    {
        _network.Enqueue((short)message.ProtocolId, message);
    }

    public void CreateTimeEvent<T>(DateTime deadline, T notation, bool isRecurring = false)
        where T : TimerNotification
    {
        _timerService.ScheduleEvent(deadline, notation, isRecurring);
    }

    public void CreateTimeEvent<T>(TimeSpan interval, T notation, bool isRecurring = false)
        where T : TimerNotification
    {
        _timerService.ScheduleEvent(interval, notation, isRecurring);
    }

    public bool GetDataTable<T>(out Dictionary<int, T> dataTable)
        where T : IDataEntity, new()
    {
        return _config.GetDataTable(out dataTable);
    }

    public T? GetSingleData<T>(int dataId)
        where T : IDataEntity, new()
    {
        return _config.GetSingleData<T>(dataId);
    }

    public int Random(int ceil)
    {
        int floor = 0;
        if (ceil < 0)
        {
            floor = ceil;
            ceil = 0;
        }

        return Random(floor, ceil);
    }

    public int Random(int floor, int ceil) 
    {  
        return _random.Next(floor, ceil);
    }

    public void RemoveTimeEvent(Guid eventId)
    {
        _timerService.RemoveEvent(eventId);
    }

    public void Send<T>(Guid playerId, short protocolId, T message)
        where T : INotificationProtocol
    {
        _network.Enqueue(playerId, protocolId, message);
    }

    public void WriteLog(LogLevelType level, string log)
    {
        switch (level)
        {
            case LogLevelType.Notice:
                _logger.Information(log);
                break;
            case LogLevelType.Warning:
                _logger.Warning(log);
                break;
            case LogLevelType.Error:
                _logger.Error(log);
                break;
            default:
                break;
        }
    }
}