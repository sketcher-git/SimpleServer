namespace Domain.Items;

public interface IItemRepository
{
    void Delete(Item item);

    Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    //Task<Dictionary<Guid, Item>?> GetListByOwnerIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Insert(Item item);

    void Update(Item item, string fieldName);
}