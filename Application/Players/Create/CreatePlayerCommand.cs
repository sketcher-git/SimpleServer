using Application.Abstractions.Messaging;
using Protocols;
using SharedKernel;

namespace Application.Players.Create;

[ProtocolAttribute(ProtocolId.CreatePlayer)]
public sealed record CreatePlayerCommand(Guid PlayerId, string Email, string Name)
    : ICommand<PlayerCreateResponseProtocol>;