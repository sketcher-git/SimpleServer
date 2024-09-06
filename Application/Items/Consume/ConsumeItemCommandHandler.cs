using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Data;
using Domain.Items;
using Protocols;
using SharedKernel;

namespace Application.Items.Consume;

internal class ConsumeItemCommandHandler(IItemRepository itemRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<ConsumeItemCommand, ConsumeItemResponseProtocol>
{
    public async Task<Result<ConsumeItemResponseProtocol>> Handle(ConsumeItemCommand command, CancellationToken cancellationToken)
    {
        var response = new ConsumeItemResponseProtocol();
        var playerId = command.PlayerId;
        var itemId = command.ItemId;
        var itemMap = OnlineCacheController.Instance.GetItemMapByPlayerId(playerId);
        if (itemMap == null)
        {
            response.ErrorType = ErrorType.Validation;
            return Result.Failure(ItemErrors.Unloaded(playerId), response);
        }

        var record = OnlineCacheController.Instance.GetItemByItemId(playerId, itemId);
        if (record == null)
        {
            response.ErrorType = ErrorType.NotFound;
            response.Item = (itemId, 0, 0);
            return Result.Failure(ItemErrors.NonExistent(itemId), response);
        }

        if (record.ItemCount < command.Amount)
        {
            response.ErrorType = ErrorType.Failure;
            response.Item = (itemId, record.DataId, record.ItemCount);
            return Result.Failure(ItemErrors.Insufficient(itemId), response);
        }

        record.ItemCount -= command.Amount;
        if (record.ItemCount > 0)
            itemRepository.Update(record, nameof(record.ItemCount));
        else
        {
            OnlineCacheController.Instance.RemoveItemByItemId(playerId, itemId);
            itemRepository.Delete(record);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        response.ErrorType = ErrorType.None;
        response.Item = (itemId, record.DataId, record.ItemCount);
        return response;
    }
}