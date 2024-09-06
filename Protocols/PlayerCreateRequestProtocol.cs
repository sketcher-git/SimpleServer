using MessagePack;
using SharedKernel;
using SharedKernel.Protocols;

namespace Protocols;

[MessagePackObject]
public sealed record PlayerCreateRequestProtocol : BaseProtocol, IRequestProtocol
{
    [IgnoreMember]
    public override ProtocolId ProtocolId => ProtocolId.CreatePlayer;

    [Key(0)]
    public string Email { get; set; }

    [Key(1)]
    public string Name { get; set; }
}