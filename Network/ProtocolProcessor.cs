using MessagePack;
using SharedKernel;
using SharedKernel.Protocols;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Network;

internal class ProtocolProcessor
{
    private static readonly Lazy<ProtocolProcessor> _instance = new Lazy<ProtocolProcessor>(() => new ProtocolProcessor());

    private readonly Dictionary<ProtocolId, Type> _requestProtocolMap;

    internal static ProtocolProcessor Instance => _instance.Value;

    private ProtocolProcessor()
    {
        _requestProtocolMap = new Dictionary<ProtocolId, Type>();
        RegisterRequestProtocolTypes();
    }

    internal object? DeserializeRequestProtocol(ProtocolId protocolId, byte[] messageBody)
    {
        if (!TryGetProtocolTypeValue(protocolId, out var protocolType))
        {
            throw new NotSupportedException($"Unsupported protocol ID: {protocolId}");
        }

        return MessagePackSerializer.Deserialize(protocolType, messageBody);
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

    private bool TryGetProtocolTypeValue(ProtocolId protocolId, out Type value)
    {
        value = default;
        ref var valueOrNull = ref CollectionsMarshal.GetValueRefOrNullRef(_requestProtocolMap, protocolId);
        if (Unsafe.IsNullRef(valueOrNull))
            return false;

        value = valueOrNull;
        return true;
    }
}