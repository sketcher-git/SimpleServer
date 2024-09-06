using MessagePack;
using SharedKernel;
using SharedKernel.Protocols;

namespace Protocols;

[MessagePackObject]
public sealed record ChatRequestProtocol : BaseProtocol, IRequestProtocol
{
    [IgnoreMember]
    public override ProtocolId ProtocolId => ProtocolId.Chat;

    [Key(0)]
    public ChatType Channel { get; set; }

    [Key(1)]
    public Guid TargetId { get; set; }

    [Key(2)]
    public string Content { get; set; }
}