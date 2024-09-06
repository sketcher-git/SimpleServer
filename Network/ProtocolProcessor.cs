using MessagePack;
using SharedKernel;
using SharedKernel.Protocols;
using System.Collections.Concurrent;

namespace Network;

internal class ProtocolProcessor
{
    private static readonly Lazy<ProtocolProcessor> _instance = new Lazy<ProtocolProcessor>(() => new ProtocolProcessor());

    private readonly ConcurrentDictionary<ProtocolId, Type> _requestProtocolMap;

    internal static ProtocolProcessor Instance => _instance.Value;

    private ProtocolProcessor()
    {
        _requestProtocolMap = new ConcurrentDictionary<ProtocolId, Type>();
        RegisterRequestProtocolTypes();
    }

    internal IRequestProtocol DeserializeRequestProtocol(ProtocolId protocolId, byte[] messageBody)
    {
        if (!_requestProtocolMap.TryGetValue(protocolId, out var protocolType))
        {
            throw new NotSupportedException($"Unsupported protocol ID: {protocolId}");
        }

        return (IRequestProtocol)MessagePackSerializer.Deserialize(protocolType, messageBody);
    }

    private void RegisterRequestProtocolTypes()
    {
        var baseType = typeof(BaseProtocol);
        var interfaceType = typeof(IRequestProtocol);
        var assembly = Protocols.AssemblyReference.Assembly;
        var messageTypes = assembly
                                   .GetTypes()
                                   .Where(t => t.IsSubclassOf(baseType)
                                       && !t.IsAbstract
                                       && interfaceType.IsAssignableFrom(t));

        foreach (var type in messageTypes)
        {
            var instance = (BaseProtocol?)Activator.CreateInstance(type);
            if(instance != null)
                _requestProtocolMap[instance.ProtocolId] = type;
        }
    }
}