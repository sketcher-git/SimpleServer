using Application.Abstractions.Config;
using SharedKernel;
using SharedKernel.Protocols;

namespace Application.Abstractions.Services;

public interface IServiceApi
{
    public void Broadcast<T>(T message)
        where T : BaseProtocol, INotificationProtocol, new();
    public void CreateTimeEvent<T>(DateTime deadline, T notation, bool isRecurring = false)
        where T : TimerNotification;
    public void CreateTimeEvent<T>(TimeSpan interval, T notation, bool isRecurring = false)
        where T : TimerNotification;
    public bool GetDataTable<T>(out Dictionary<int, T> dataTable)
        where T : IDataEntity, new();
    public T? GetSingleData<T>(int dataId)
        where T : IDataEntity, new();
    public int Random(int ceil);
    public int Random(int floor, int ceil);
    public void RemoveTimeEvent(Guid eventId);
    public void Send<T>(Guid playerId, short protocolId, T message)
        where T : INotificationProtocol;
    public void WriteLog(LogLevelType level, string log);
}