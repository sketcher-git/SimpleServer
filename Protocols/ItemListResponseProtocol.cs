using MessagePack;
using SharedKernel;
using SharedKernel.Protocols;

namespace Protocols;

[MessagePackObject]
public sealed record ItemListResponseProtocol : BaseProtocol, IResponseProtocol
{
    [IgnoreMember]
    public override ProtocolId ProtocolId => ProtocolId.ItemList;

    [Key(0)]
    public ErrorType ErrorType { get; set; }

    [Key(1)]
    public Dictionary<Guid, (Guid, int, int)>? ItemMap { get; set; }
}