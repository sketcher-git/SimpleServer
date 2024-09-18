using NetCoreServer;
using System.Net.Sockets;
using System.Net;
using SharedKernel;

namespace Network;

public class SimpleTcpServer : TcpServer
{
    private const int _maxDegreeOfParallelism = 64;
    private readonly List<Task> _broadcastTaskList = new List<Task>();

    private SemaphoreSlim? _semaphore;

    public Action? OnServerStarted;
    public Action? OnServerStopped;

    public SimpleTcpServer(IPAddress address, int port)
        : base(address, port) { }

    protected override TcpSession CreateSession() => new SimpleTcpSession(this);

    public async Task MulticastAsync(byte[] buffer)
    {
        _semaphore = new SemaphoreSlim(_maxDegreeOfParallelism);
        _broadcastTaskList.Clear();
        foreach (var item in Sessions)
        {
            var session = item.Value;
            if (!session.IsConnected || session.IsDisposed)
                continue;

            await _semaphore.WaitAsync();
            _broadcastTaskList.Add(Task.Run(() =>
            {
                try
                {
                    session.SendAsync(buffer);
                }
                finally
                {
                    _semaphore.Release();
                }
            }));
        }

        await Task.WhenAll(_broadcastTaskList);
    }

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