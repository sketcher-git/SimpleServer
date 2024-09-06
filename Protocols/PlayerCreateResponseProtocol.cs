using MessagePack;
using SharedKernel;
using SharedKernel.Protocols;

namespace Protocols;

[MessagePackObject]
public sealed record PlayerCreateResponseProtocol : BaseProtocol, IResponseProtocol
{
    [IgnoreMember]
    public override ProtocolId ProtocolId => ProtocolId.CreatePlayer;

    [Key(0)]
    public ErrorType ErrorType { get; set; }

    [Key(1)]
    public Guid PlayerId { get; set; }

    [Key(2)]
    public string Email { get; set; }

    [Key(3)]
    public string Name { get; set; }
}