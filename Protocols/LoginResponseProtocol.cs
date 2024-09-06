using MessagePack;
using SharedKernel;
using SharedKernel.Protocols;

namespace Protocols;

[MessagePackObject]
public sealed record LoginResponseProtocol : BaseProtocol, IResponseProtocol
{
    [IgnoreMember]
    public override ProtocolId ProtocolId => ProtocolId.Login;

    [Key(0)]
    public ErrorType ErrorType { get; set; }

    [Key(1)]
    public long LoginTimestamp { get; set; }
}