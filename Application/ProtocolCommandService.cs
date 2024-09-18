using Application.Abstractions.Services;
using SharedKernel;
using System.Collections.Concurrent;
using System.Reflection;

namespace Application;

public sealed class ProtocolCommandService : IProtocolCommandService
{
    private static readonly ConcurrentDictionary<ProtocolId, Type> _protocolCommandMap = new ConcurrentDictionary<ProtocolId, Type>();

    public ProtocolCommandService()
    {
        InitializeProtocolCommandMap();
    }

    private void InitializeProtocolCommandMap()
    {
        var commandTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => Attribute.IsDefined(t, typeof(ProtocolAttribute)));

        var spanCommandType = commandTypes.ToArray().AsSpan();
        int spanLength = spanCommandType.Length;
        for (int i = 0; i < spanLength; i++)
        {
            var attribute = (ProtocolAttribute?)spanCommandType[i]
                .GetCustomAttributes(typeof(ProtocolAttribute), false)
                .FirstOrDefault();
            if (attribute != null)
                _protocolCommandMap[attribute.ProtocolId] = spanCommandType[i];
        }
    }

    public Type? GetCommandByProtocolId(ProtocolId protocolId)
    {
        _protocolCommandMap.TryGetValue(protocolId, out var commandType);
        return commandType;
    }

    public ConcurrentDictionary<ProtocolId, Type> GetProtocolCommandMap()
    { 
        return _protocolCommandMap;
    }
}