using MessagePack;
using SharedKernel;
using SharedKernel.Protocols;

namespace Protocols;

[MessagePackObject]
public sealed record HeartbeatNotificationProtocol : BaseProtocol, INotificationProtocol
{
    [IgnoreMember]
    public override ProtocolId ProtocolId => ProtocolId.HeartbeatNotification;

    [Key(0)]
    public long NowTimestamp { get; set; }
}