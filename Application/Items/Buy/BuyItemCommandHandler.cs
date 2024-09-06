using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.ConfigEntity;
using Application.Data;
using Application.Items.Loot;
using Domain.Items;
using Protocols;
using SharedKernel;

namespace Application.Items.Buy;

internal sealed class BuyItemCommandHandler(IItemRepository itemRepository, IServiceApi api, IUnitOfWork unitOfWork)
    : ICommandHandler<BuyItemCommand, BuyItemResposeProtocol>
{
    public async Task<Result<BuyItemResposeProtocol>> Handle(BuyItemCommand command, CancellationToken cancellationToken)
    {
        var response = new BuyItemResposeProtocol();
        var dataId = command.DataId;
        var itemData = api.GetSingleData<item_cfg>(dataId);
        if (itemData == null)
        {
            response.ErrorType = ErrorType.NotFound;
            return Result.Failure(ItemErrors.NonExistent(dataId), response);
        }

        var playerId = command.PlayerId;
        var amout = command.Amount;
        var itemMap = OnlineCacheController.Instance.GetItemMapByPlayerId(playerId);
        if (itemMap == null)
        {
            response.ErrorType = ErrorType.Validation;
            return Result.Failure(ItemErrors.Unloaded(playerId), response);
        }

        if (amout < 1)
        {
            response.ErrorType = ErrorType.Failure;
            return Result.Failure(ItemErrors.InvalidAmount(playerId, amout), response);
        }

        var record = OnlineCacheController.Instance.GetItemByDataId(playerId, dataId);
        if (record != null)
        {
            record.ItemCount += amout;
            itemRepository.Update(record, nameof(record.ItemCount));
        }
        else
        {
            record = Item.Create(dataId, amout, playerId);
            itemRepository.Insert(record);
            itemMap.Add(record.Id, record);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        response.ErrorType = ErrorType.None;
        response.Item = (record.Id, record.DataId, record.ItemCount);
        return response;
    }
}