using MessagePack;
using SharedKernel;
using SharedKernel.Protocols;

namespace Protocols;

[MessagePackObject]
public sealed record PlayerInformationResponseProtocol : BaseProtocol, IResponseProtocol
{
    [IgnoreMember]
    public override ProtocolId ProtocolId => ProtocolId.PlayerInfo;

    [Key(0)]
    public ErrorType ErrorType { get; set; }

    [Key(1)]
    public (Guid playerId, string playerName, long loginTimestamp) PlayerInfo { get; set; }
}