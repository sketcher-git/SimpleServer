using Application.Abstractions.Messaging;
using Protocols;
using SharedKernel;

namespace Application.Items.Consume;

[ProtocolAttribute(ProtocolId.ConsumeItem)]
public sealed record ConsumeItemCommand(Guid PlayerId, Guid ItemId, int Amount)
    : ICommand<ConsumeItemResponseProtocol>;