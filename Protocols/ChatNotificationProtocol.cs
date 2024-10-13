using MessagePack;
using SharedKernel;
using SharedKernel.Protocols;

namespace Protocols;

[MessagePackObject]
public sealed record ChatNotificationProtocol : BaseProtocol, INotificationProtocol
{
    [IgnoreMember]
    public override ProtocolId ProtocolId => ProtocolId.ChatNotification;

    [Key(0)]
    public Guid SenderId { get; set; }

    [Key(1)]
    public ChatType Channel { get; set; }

    [Key(2)]
    public string SenderName { get; set; }

    [Key(3)]
    public string Content { get; set; }

    [Key(4)]
    public DateTime SendingTime { get; set; }
}