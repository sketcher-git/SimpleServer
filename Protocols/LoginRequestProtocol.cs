using MessagePack;
using SharedKernel;
using SharedKernel.Protocols;

namespace Protocols;

[MessagePackObject]
public sealed record LoginRequestProtocol : BaseProtocol, IRequestProtocol
{
    [IgnoreMember]
    public override ProtocolId ProtocolId => ProtocolId.Login;
}