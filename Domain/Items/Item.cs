using SharedKernel;

namespace Domain.Items;

public sealed class Item : Entity
{
    private Item(Guid id, int dataId, int itemCount, Guid ownerId) : base(id)
    {
        DataId = dataId;
        ItemCount = itemCount;
        OwnerId = ownerId;
    }

    private Item()
    {
    }

    public int DataId {  get; private set; }
    public int ItemCount {  get; set; }
    public Guid OwnerId { get; private set; }

    public static Item Create(int dataId, int itemCount, Guid ownerId)
    {
        var item = new Item(Guid.NewGuid(), dataId, itemCount, ownerId);

        return item;
    }
}