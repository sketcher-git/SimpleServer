using MessagePack;
using Microsoft.Extensions.Configuration;
using Serilog;
using SharedKernel;
using SharedKernel.Protocols;
using System.Collections.Concurrent;
using System.Net;

namespace Network;

public sealed partial class NetworkManager : INetworkService
{
    private static readonly BlockingCollection<ResponseInfomation> _responsesQueue = new BlockingCollection<ResponseInfomation>();
    private readonly IConfiguration _configuration;
    private readonly SimpleTcpServer _server;

    private volatile bool _isServerStarted = false;

    public NetworkManager(IConfiguration configurationm)
    {
        _configuration = configurationm;
        int port = _configuration.GetValue<int>("ServerSettings:Port");
        _server = new SimpleTcpServer(IPAddress.Any, port);
        _server.OnServerStarted += () => _isServerStarted = true;
        _server.OnServerStopped += () => _isServerStarted = false;
        StartSend();
    }

    public void Enqueue<T>(short protocolId, T message, ResponseType responseType = ResponseType.Broadcast)
        where T : INotificationProtocol
    {
        Enqueue(Guid.Empty, protocolId, message, responseType);
    }

    public void Enqueue<T>(Guid playerId, short protocolId, T message, ResponseType responseType = ResponseType.Common)
        where T : INotificationProtocol
    {
        if (!_isInService)
            return;

        var response = new ResponseInfomation
        {
            PlayerId = playerId,
            ProtocolId = protocolId,
            Protocol = message,
            ResponseType = responseType
        };

        _responsesQueue.Add(response);
    }

    public SimpleTcpServer GetTcpServer()
    {
        return _server;
    }

    public RequestInfomation? TakeRequest()
    {
        if (!_requestsQueue.TryTake(out var request))
        {
            return null;
        }

        return request;
    }
}

public sealed partial class NetworkManager
{
    internal const int HeaderSize = 4;
    internal const int ProtocolIdSize = 2;
    private static readonly ConcurrentDictionary<Guid, SimpleTcpSession> _PlayerSessionMap = new ConcurrentDictionary<Guid, SimpleTcpSession>();
    private static readonly BlockingCollection<RequestInfomation> _requestsQueue = new BlockingCollection<RequestInfomation>();

    private static volatile bool _isInService = false;

    private async Task Broadcast(byte[] buffer)
    {
        await _server.MulticastAsync(buffer);
    }

    internal static void NetworkLog(LogLevelType level, string log)
    {
        Task.Run(() =>
        {
            string networkLog = $"NETWORKLOG--->{log}<---";
            switch (level)
            {
                case LogLevelType.Notice:
                    Log.Logger.Information(networkLog);
                    break;
                case LogLevelType.Warning:
                    Log.Logger.Warning(networkLog);
                    break;
                case LogLevelType.Error:
                    Log.Logger.Error(networkLog);
                    break;
                default:
                    break;
            }
        });
    }

    private byte[] PackMessage(short protocolId, object protocol, Guid playerId)
    {
        byte[] messageBody = MessagePackSerializer.Serialize(protocol);

        byte[] protocolIdBytes = BitConverter.GetBytes(protocolId);

        byte[] playerIdBytes = playerId.ToByteArray();

        int lenth = protocolIdBytes.Length + messageBody.Length + playerIdBytes.Length;
        byte[] size = BitConverter.GetBytes(lenth);
        byte[] finalMessage = new byte[lenth + HeaderSize];
        Buffer.BlockCopy(size, 0, finalMessage, 0, HeaderSize);
        Buffer.BlockCopy(protocolIdBytes, 0, finalMessage, HeaderSize, protocolIdBytes.Length);
        Buffer.BlockCopy(messageBody, 0, finalMessage, HeaderSize + protocolIdBytes.Length, messageBody.Length);
        Buffer.BlockCopy(playerIdBytes, 0, finalMessage, HeaderSize + protocolIdBytes.Length + messageBody.Length, playerIdBytes.Length);

        return finalMessage;
    }

    internal static void Receive(Guid playerId, object message)
    {
        var request = new RequestInfomation
        {
            PlayerId = playerId,
            Protocol = message
        };
        _requestsQueue.Add(request);
    }

    internal static bool RegisterPlayerSession(Guid playerId, SimpleTcpSession session)
    {
        bool isDone = false;
        if (_PlayerSessionMap.TryGetValue(playerId, out var oldSession)
        && oldSession.IsConnected)
        {
            NetworkLog(LogLevelType.Error, $"Cannot add a new session to player {playerId}, when its old session is connecting!");
            session.Disconnect();
            return false;
        }

        if (oldSession != null
        && (!oldSession.IsConnected || oldSession.IsDisposed)
        && !oldSession.Disconnect())
        {
            UnregisterPlayerSession(playerId);
        }

        if (!(isDone = _PlayerSessionMap.TryAdd(playerId, session)))
            NetworkLog(LogLevelType.Error, $"Cannot add the session to player {playerId}!");
        else
            _isInService = true;

        return isDone;
    }

    private void Send(Guid playerId, byte[] buffer)
    {
        if (_PlayerSessionMap.TryGetValue(playerId, out var session)
            && session != null)
        {
            session.SendAsync(buffer);
        }
    }

    private void StartSend()
    {
        Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                try
                {
                    if (!_isServerStarted)
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    if (!_responsesQueue.TryTake(out var response))
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    if (!_isInService)
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    byte[] buffer = PackMessage(response.ProtocolId, response.Protocol, response.PlayerId);
                    if (buffer == null)
                        continue;

                    switch (response.ResponseType)
                    {
                        case ResponseType.Common:
                            Send(response.PlayerId, buffer);
                            break;
                        case ResponseType.Broadcast:
                            await Broadcast(buffer);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    NetworkLog(LogLevelType.Error, e.Message);
                }
            }
        });
    }

    internal static (ProtocolId protocolId, object? protocol, Guid playerId) UnpackMessage(byte[] packedMessage)
    {
        var protocolId = (ProtocolId)BitConverter.ToInt16(packedMessage, 0);

        var playerId = new Guid(packedMessage.Skip(packedMessage.Length - 16).ToArray());

        byte[] messageBody = packedMessage.Skip(2).Take(packedMessage.Length - 18).ToArray();

        var protocol = ProtocolProcessor.Instance.DeserializeRequestProtocol(protocolId, messageBody);

        return (protocolId, protocol, playerId);
    }

    internal static bool UnregisterPlayerSession(Guid playerId)
    {
        bool isDone = false;
        if (isDone = _PlayerSessionMap.TryRemove(playerId, out _))
            _isInService = !_PlayerSessionMap.IsEmpty;

        return isDone;
    }
}