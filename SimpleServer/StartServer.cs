using Application.Abstractions.Services;
using MediatR;
using MessagePack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Network;
using SharedKernel.Protocols;
using SharedKernel;
using Serilog;
using Application.Timer;
using System.Reflection;
using Application.Abstractions.Data;

namespace SimpleServer;

internal class StartServer : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly IMediator _mediator;
    private readonly INetworkService _network;
    private readonly NotificationDispatcher _notificationDispatcher;
    private readonly IProtocolCommandService _protocolCommand;
    private readonly IServiceApi _serviceApi;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    public StartServer(IConfiguration configuration, IMediator mediator, INetworkService network, NotificationDispatcher notificationDispatcher, IProtocolCommandService protocolCommand, IServiceApi serviceApi)
    {
        _configuration = configuration;
        _mediator = mediator;
        _network = network;
        _notificationDispatcher = notificationDispatcher;
        _protocolCommand = protocolCommand;
        _serviceApi = serviceApi;
    }

    public Task StartAsync(CancellationToken token)
    {
        return StartTcpServer(token);
    }

    private async Task StartProcessRequests(CancellationToken token)
    {
        var commandTypeCache = _protocolCommand.GetProtocolCommandMap().ToDictionary();
        var constructorCache = new Dictionary<Type, ConstructorInfo>();

        while (_mediator != null)
        {
            try
            {
                var request = _network.TakeRequest();
                if (request == null)
                {
                    Thread.Sleep(100);
                    continue;
                }

                var playerId = request.PlayerId;
                var protocol = (BaseProtocol)request.Protocol;
                var protocolId = protocol.ProtocolId;
                if (!commandTypeCache.TryGetValue(protocolId, out var commandType))
                {
                    await Task.Delay(100, token);
                    continue;
                }

                var properties = protocol.GetType()
                                                .GetProperties()
                                                .Where(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Length > 0)
                                                .ToArray();
                int length = properties.Length + 1;
                Type[] fieldsType = new Type[length];
                object[] values = new object[length];
                fieldsType[0] = playerId.GetType();
                values[0] = playerId;
                for (int i = 0; i < length - 1; i++)
                {
                    var p = properties[i];
                    fieldsType[i + 1] = p.PropertyType;
                    values[i + 1] = p.GetValue(protocol);
                }
                
                if (!constructorCache.TryGetValue(commandType, out var constructor))
                {
                    constructor = commandType.GetConstructor(fieldsType);
                    if (constructor == null)
                    {
                        Log.Logger.Error($"CommandType {commandType.Name} cannot get constructor!!");
                        continue;
                    }
                    constructorCache[commandType] = constructor;
                }

                var commandInstance = constructor.Invoke(values);
                var result = (Result?)await _mediator.Send(commandInstance);
                if (result == null)
                    continue;

                var valueType = result.GetType()
                            .GetProperty("Value");
                if (valueType == null)
                    continue;

                try
                {
                    var value = valueType.GetValue(result);
                    if (value != null)
                        _serviceApi.Send(playerId, (short)protocolId, (IResponseProtocol)value);
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e.Message);
                    Log.Logger.Error(e.StackTrace);
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message);
                Log.Logger.Error(e.StackTrace);
            }
        }
    }

    private async Task StartTcpServer(CancellationToken token)
    {
        var server = _network.GetTcpServer();
        server.OptionReuseAddress = true;
        Log.Logger.Information("Server starting...");
        server.Start();
        StartLogicThread();
        Log.Logger.Information("Server started");

        _notificationDispatcher.StartDispatch();
        _serviceApi.CreateTimeEvent(TimeSpan.FromSeconds(5), new HeartbeatNotification(), true);

        for (; ; )
        {
            string line = Console.ReadLine();
            if (line == "serverExit")
                break;
        }

        Log.Logger.Information("Server stopping...");
        server.Stop();
        Log.Logger.Information("Done!");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        return Task.CompletedTask;
    }

    private void StartLogicThread()
    {
        Task.Run(() => StartProcessRequests(_cts.Token));
        Log.Logger.Information("Logic thread started!");
    }
}