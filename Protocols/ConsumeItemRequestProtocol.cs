using MessagePack;
using SharedKernel;
using SharedKernel.Protocols;

namespace Protocols;

[MessagePackObject]
public sealed record ConsumeItemRequestProtocol : BaseProtocol, IRequestProtocol
{
    [IgnoreMember]
    public override ProtocolId ProtocolId => ProtocolId.ConsumeItem;

    [Key(0)]
    public Guid ItemId { get; set; }

    [Key(1)]
    public int Amount { get; set; }
}