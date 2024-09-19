using MessagePack;
using SharedKernel;
using SharedKernel.Protocols;

namespace Protocols;

[MessagePackObject]
public sealed record ConsumeItemResponseProtocol : BaseProtocol, IResponseProtocol
{
    [IgnoreMember]
    public override ProtocolId ProtocolId => ProtocolId.ConsumeItem;

    [Key(0)]
    public ErrorType ErrorType { get; set; }

    [Key(1)]
    public (Guid itemId, int itemDataId, int itemAmount) Item { get; set; }
}