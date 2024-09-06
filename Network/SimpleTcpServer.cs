using NetCoreServer;
using System.Net.Sockets;
using System.Net;
using SharedKernel;

namespace Network;

public class SimpleTcpServer : TcpServer
{
    public Action? OnServerStarted;
    public Action? OnServerStopped;

    public SimpleTcpServer(IPAddress address, int port)
        : base(address, port) { }

    protected override TcpSession CreateSession() => new SimpleTcpSession(this);

    protected override void OnError(SocketError error)
    {
        NetworkManager.NetworkLog(LogLevelType.Error, $"Server caught an error with code {error}");
    }

    protected override void OnStarted()
    {
        base.OnStarted();
        OnServerStarted?.Invoke();
    }

    protected override void OnStopped()
    {
        base.OnStopped();
        OnServerStopped?.Invoke();
    }
}