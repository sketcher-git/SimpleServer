using System.Net.Sockets;
using NetCoreServer;
using Protocols;
using SharedKernel;

namespace Network;

public class SimpleTcpSession : TcpSession
{
    private Guid _playerId;

    public SimpleTcpSession(TcpServer server)
        : base(server) {}

    protected override void OnConnected()
    {
        NetworkManager.NetworkLog(LogLevelType.Notice, $"Client TCP session with Id {Id} connected!");
    }

    protected override void OnDisconnected()
    {
        if (_playerId != Guid.Empty)
        {
            NetworkManager.UnregisterPlayerSession(_playerId);
            NetworkManager.Receive(_playerId, new LogOutRequestProtocol());
        }

        NetworkManager.NetworkLog(LogLevelType.Notice, $"Client TCP session with Id {Id} and PlayerId {_playerId} disconnected!");
    }

    protected override void OnError(SocketError error)
    {
        NetworkManager.NetworkLog(LogLevelType.Notice, $"Session caught an error with code {error}");
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        int processedBytes = 0;
        while (processedBytes < size)
        {
            if (size - processedBytes < NetworkManager.HeaderSize)
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    NetworkManager.NetworkLog(LogLevelType.Error, "Received zero head!");
                };
            }

            int messageLength = BitConverter.ToInt32(buffer, (int)offset + processedBytes);
            if (size - processedBytes - NetworkManager.HeaderSize < messageLength)
            {
                break;
            }

            processedBytes += NetworkManager.HeaderSize + messageLength;

            byte[] packedMessage = new byte[messageLength];
            Array.Copy(buffer, (int) offset + NetworkManager.HeaderSize, packedMessage, 0, messageLength);
            var message = NetworkManager.UnpackMessage(packedMessage);
            if (message.playerId == Guid.Empty)
            {
                Disconnect();
                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    NetworkManager.NetworkLog(LogLevelType.Error, "PlayerId is empty!");
                };
            }

            if (message.protocolId == ProtocolId.Login)
            {
                if (NetworkManager.RegisterPlayerSession(message.playerId, this))
                    _playerId = message.playerId;
                else 
                    break;
            }

            if (message.protocol == null)
            {
                NetworkManager.NetworkLog(LogLevelType.Error, $"Message with protocolId {message.protocolId.ToString()} has a null protocol!");
                break;
            }

            NetworkManager.Receive(message.playerId, message.protocol);
        }
    }
}