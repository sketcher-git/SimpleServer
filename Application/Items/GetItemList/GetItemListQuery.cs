using Application.Abstractions.Messaging;
using Protocols;
using SharedKernel;

namespace Application.Items.GetItemList;

[ProtocolAttribute(ProtocolId.ItemList)]
public sealed record GetItemListQuery(Guid PlayerId)
    : IQuery<ItemListResponseProtocol>;