using MessagePack;
using SharedKernel;
using SharedKernel.Protocols;

namespace Protocols;

[MessagePackObject]
public sealed record ChatResponseProtocol : BaseProtocol, IResponseProtocol
{
    [IgnoreMember]
    public override ProtocolId ProtocolId => ProtocolId.Chat;

    [Key(0)]
    public ErrorType ErrorType { get; set; }

    [Key(1)]
    public ChatType Channel { get; set; }

    [Key(2)]
    public Guid TargetId { get; set; }
}