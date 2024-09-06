using Application.Abstractions.Caching;
using Protocols;
using SharedKernel;

namespace Application.Players.GetById;

[ProtocolAttribute(ProtocolId.PlayerInfo)]
public sealed record GetPlayerByIdQuery(Guid PlayerId, Guid TargetPlayerId)
    : ICachedQuery<PlayerInformationResponseProtocol>
{
    public string CacheKey => $"player-by-id-{TargetPlayerId}";

    public TimeSpan? Expiration => null;
}