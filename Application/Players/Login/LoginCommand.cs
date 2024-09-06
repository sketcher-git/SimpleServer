using Application.Abstractions.Messaging;
using Protocols;
using SharedKernel;

namespace Application.Players.Login;

[ProtocolAttribute(ProtocolId.Login)]
public sealed record LoginCommand(Guid PlayerId) : ICommand<LoginResponseProtocol>;