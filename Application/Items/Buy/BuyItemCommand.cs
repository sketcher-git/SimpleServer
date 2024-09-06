using Application.Abstractions.Messaging;
using Protocols;
using SharedKernel;

namespace Application.Items.Loot;

[ProtocolAttribute(ProtocolId.BuyItem)]
public sealed record BuyItemCommand(Guid PlayerId, int DataId, int Amount)
    : ICommand<BuyItemResposeProtocol>;