using Application.Abstractions.Services;
using MediatR;
using MessagePack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Network;
using SharedKernel.Protocols;
using SharedKernel;
using Application.Timer;
using System.Reflection;

namespace SimpleServer;

internal class StartServer : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly IMediator _mediator;
    private readonly INetworkService _network;
    private readonly NotificationDispatcher _notificationDispatcher;
    private readonly IProtocolCommandService _protocolCommand;
    private readonly IServiceApi _serviceApi;

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
        return Task.Run(() => StartTcpServer(token));
    }

    private async Task StartProcessRequests(CancellationToken token)
    {
        var commandTypeCache = _protocolCommand.GetProtocolCommandMap().ToDictionary();
        var constructorCache = new Dictionary<Type, ConstructorInfo>();
        var keyProperties = new List<PropertyInfo>();
        _serviceApi.WriteLog(LogLevelType.Notice, "Logic thread started!");

        while (_mediator != null)
        {
            try
            {
                var request = _network.TakeRequest();
                if (request == null)
                {
                    await Task.Delay(100, token);
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
                         .GetProperties();
                keyProperties.Clear();
                int length = properties.Length;
                for(int i = 0; i< length; i++)
                {
                    if (Attribute.IsDefined(properties[i], typeof(KeyAttribute)))
                        keyProperties.Add(properties[i]);
                }

                int count = keyProperties.Count + 1;
                Type[] fieldsType = new Type[count];
                object[] values = new object[count];
                fieldsType[0] = playerId.GetType();
                values[0] = playerId;
                for (int i = 0; i < length - 1; i++)
                {
                    var p = keyProperties[i];
                    fieldsType[i + 1] = p.PropertyType;
                    values[i + 1] = p.GetValue(protocol);
                }
                
                if (!constructorCache.TryGetValue(commandType, out var constructor))
                {
                    constructor = commandType.GetConstructor(fieldsType);
                    if (constructor == null)
                    {
                        _serviceApi.WriteLog(LogLevelType.Error, $"CommandType {commandType.Name} cannot get constructor!!");
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
                    _serviceApi.WriteLog(LogLevelType.Error, e.Message);
                    _serviceApi.WriteLog(LogLevelType.Error, e.StackTrace);
                }
            }
            catch (Exception e)
            {
                _serviceApi.WriteLog(LogLevelType.Error, e.Message);
                _serviceApi.WriteLog(LogLevelType.Error, e.StackTrace);
            }
        }
    }

    private void StartTcpServer(CancellationToken token)
    {
        var server = _network.GetTcpServer();
        server.OptionReuseAddress = true;
        _serviceApi.WriteLog(LogLevelType.Notice, "Server starting...");
        server.Start();
        Task.Factory.StartNew(async () => await StartProcessRequests(token));
        _notificationDispatcher.StartDispatchNotifications();
        _serviceApi.WriteLog(LogLevelType.Notice, "Server started");

        _serviceApi.CreateTimeEvent(TimeSpan.FromSeconds(5), new HeartbeatNotification(), true);

        for (; ; )
        {
            string line = Console.ReadLine();
            if (line == "serverExit")
                break;
        }

        _serviceApi.WriteLog(LogLevelType.Notice, "Server stopping...");
        server.Stop();
        _serviceApi.WriteLog(LogLevelType.Notice, "Done!");
    }

    public Task StopAsync(CancellationToken token)
    {
        return Task.CompletedTask;
    }
}