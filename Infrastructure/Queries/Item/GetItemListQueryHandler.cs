using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Data;
using Application.Items.GetItemList;
using Application.Players.Login;
using Domain.Items;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Protocols;
using SharedKernel;

namespace Infrastructure.Queries.Item;

internal sealed class GetItemListQueryHandler(ApplicationReadDbContext dbContext, INotificationQueue notificationQueue)
    : IQueryHandler<GetItemListQuery, ItemListResponseProtocol>
{
    public async Task<Result<ItemListResponseProtocol>> Handle(GetItemListQuery query, CancellationToken cancellationToken)
    {
        var response = new ItemListResponseProtocol();
        var playerId = query.PlayerId;
        var player = OnlineCacheController.Instance.GetPlayerOnlineCacheByPlayerId(playerId);
        if (player == null)
        {
            var error = ItemErrors.InvalidQuery(playerId);
            response.ErrorType = error.Type;
            return Result.Failure(error, response);
        }

        var itemMap = OnlineCacheController.Instance.GetItemMapByPlayerId(playerId);
        if (itemMap == null)
        {
            itemMap = await dbContext.Items.Where(item => item.OwnerId == playerId).ToDictionaryAsync(item => item.Id);
            OnlineCacheController.Instance.RegisterItemMap(playerId, itemMap);
        }

        if (itemMap != null
        && itemMap.Count > 0)
        {
            response.ItemMap = new Dictionary<Guid, (Guid, int, int)>();
            foreach (var item in itemMap)
            {
                response.ItemMap.Add(item.Key, (item.Key, item.Value.DataId, item.Value.ItemCount));
            }
        }

        notificationQueue.Enqueue(new LoginNotification(playerId, player.Name));
        return response;
    }
}