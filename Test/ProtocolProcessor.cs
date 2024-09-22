using MessagePack;
using SharedKernel;
using SharedKernel.Protocols;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Test;

internal class ProtocolProcessor
{
    private static readonly Lazy<ProtocolProcessor> _instance = new Lazy<ProtocolProcessor>(() => new ProtocolProcessor());

    private readonly Dictionary<ProtocolId, Type> _responseProtocolMap;

    internal static ProtocolProcessor Instance => _instance.Value;

    private ProtocolProcessor()
    {
        _responseProtocolMap = new Dictionary<ProtocolId, Type>();
        RegisterMessageTypes();
    }

    internal object? DeserializeMessage(ProtocolId protocolId, byte[] messageBody)
    {
        if (!TryGetProtocolTypeValue(protocolId, out var messageType))
        {
            throw new NotSupportedException($"Unsupported protocol ID: {protocolId}");
        }

        try
        {
            return MessagePackSerializer.Deserialize(messageType, messageBody);
        }
        catch (MessagePackSerializationException ex)
        {
            Console.WriteLine($"MessagePack serialization error: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
        }

        return default;
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

    private bool TryGetProtocolTypeValue(ProtocolId protocolId, out Type value)
    {
        value = default;
        ref var valueOrNull = ref CollectionsMarshal.GetValueRefOrNullRef(_responseProtocolMap, protocolId);
        if (Unsafe.IsNullRef(valueOrNull))
            return false;

        value = valueOrNull;
        return true;
    }
}