using MessagePack;
using SharedKernel;
using SharedKernel.Protocols;

namespace Protocols;

[MessagePackObject]
public sealed record BuyItemRequestProtocol : BaseProtocol, IRequestProtocol
{
    [IgnoreMember]
    public override ProtocolId ProtocolId => ProtocolId.BuyItem;

    [Key(0)]
    public int DataId { get; set; }

    [Key(1)]
    public int Amount { get; set; }
}