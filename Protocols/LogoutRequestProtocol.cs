using MessagePack;
using SharedKernel;
using SharedKernel.Protocols;

namespace Protocols;

[MessagePackObject]
public sealed record LogOutRequestProtocol : BaseProtocol, IRequestProtocol
{
    [IgnoreMember]
    public override ProtocolId ProtocolId => ProtocolId.Logout;
}