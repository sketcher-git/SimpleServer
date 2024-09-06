using MessagePack;

namespace SharedKernel.Protocols;

[MessagePackObject]
public abstract record BaseProtocol
{
    [IgnoreMember]
    public abstract ProtocolId ProtocolId { get; }
}