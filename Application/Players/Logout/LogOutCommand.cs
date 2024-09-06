using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Players.Logout;

[ProtocolAttribute(ProtocolId.Logout)]
public sealed record LogOutCommand(Guid playerId) : ICommand;