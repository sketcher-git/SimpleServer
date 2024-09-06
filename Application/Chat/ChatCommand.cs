using Application.Abstractions.Messaging;
using Protocols;
using SharedKernel;

namespace Application.Chat;

[ProtocolAttribute(ProtocolId.Chat)]
public sealed record ChatCommand(Guid PlayerId, ChatType Channel, Guid TargetId, string Content) : ICommand<ChatResponseProtocol>;