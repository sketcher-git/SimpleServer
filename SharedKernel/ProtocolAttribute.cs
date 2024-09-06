namespace SharedKernel;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ProtocolAttribute(ProtocolId protocolId) : Attribute
{
    public ProtocolId ProtocolId { get => protocolId; }
}