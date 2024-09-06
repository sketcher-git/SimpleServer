using SharedKernel.Protocols;
using SharedKernel;

namespace Network;

public interface INetworkService
{
    public void Enqueue<T>(short protocolId, T message, ResponseType responseType = ResponseType.Broadcast)
        where T : INotificationProtocol;
    public void Enqueue<T>(Guid playerId, short protocolId, T message, ResponseType responseType = ResponseType.Common)
    where T : INotificationProtocol;
    public SimpleTcpServer GetTcpServer();
    public RequestInfomation? TakeRequest();
}