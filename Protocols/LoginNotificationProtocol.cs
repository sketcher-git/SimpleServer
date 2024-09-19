using MessagePack;
using SharedKernel;
using SharedKernel.Protocols;

namespace Protocols;

[MessagePackObject]
public sealed record LoginNotificationProtocol : BaseProtocol, INotificationProtocol
{
    [IgnoreMember]
    public override ProtocolId ProtocolId => ProtocolId.LoginNotification;

    [Key(0)]
    public Guid PlayerId { get; set; }

    [Key(1)]
    public string? Name { get; set; }
}