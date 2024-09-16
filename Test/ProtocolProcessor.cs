using MessagePack;
using SharedKernel;
using SharedKernel.Protocols;
using System.Collections.Concurrent;

namespace Test;

internal class ProtocolProcessor
{
    private static readonly Lazy<ProtocolProcessor> _instance = new Lazy<ProtocolProcessor>(() => new ProtocolProcessor());

    private readonly ConcurrentDictionary<ProtocolId, Type> _responseProtocolMap;

    internal static ProtocolProcessor Instance => _instance.Value;

    private ProtocolProcessor()
    {
        _responseProtocolMap = new ConcurrentDictionary<ProtocolId, Type>();
        RegisterMessageTypes();
    }

    internal object? DeserializeMessage(ProtocolId protocolId, byte[] messageBody)
    {
        if (!_responseProtocolMap.TryGetValue(protocolId, out var messageType))
        {
            throw new NotSupportedException($"Unsupported protocol ID: {protocolId}");
        }

        return MessagePackSerializer.Deserialize(messageType, messageBody);
    }

    private void RegisterMessageTypes()
    {
        var baseType = typeof(BaseProtocol);
        var interfaceType = typeof(INotificationProtocol);
        var messageTypes = Protocols.AssemblyReference.Assembly
                                   .GetTypes()
                                   .Where(t => t.IsSubclassOf(baseType)
                                       && !t.IsAbstract
                                       && interfaceType.IsAssignableFrom(t));

        foreach (var type in messageTypes)
        {
            var instance = (BaseProtocol?)Activator.CreateInstance(type);
            _responseProtocolMap[instance.ProtocolId] = type;
        }
    }
}