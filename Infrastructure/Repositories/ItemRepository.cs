using Domain.Items;
using Domain.Players;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace Infrastructure.Repositories;

internal sealed class ItemRepository(ApplicationReadDbContext dbReadContext, ApplicationWriteDbContext dbWriteContext) : IItemRepository
{
    public void Delete(Item item)
    {
        dbWriteContext.Items.Remove(item);
    }

    public async Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbReadContext.Items.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    //public async Task<Dictionary<Guid, Item>?> GetListByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    //{
    //    return await dbReadContext.Items.Where(item => item.OwnerId == ownerId).ToDictionaryAsync(item => item.Id);
    //}

    public void Insert(Item item)
    {
        dbWriteContext.Items.Add(item);
    }

    public void Update(Item item, string fieldName)
    {
        var trackedPlayer = dbWriteContext.Players.Local.FirstOrDefault(p => p.Id == item.Id);
        if (trackedPlayer != null)
        {
            dbWriteContext.Entry(trackedPlayer).Property(fieldName).CurrentValue =
                dbWriteContext.Entry(item).Property(fieldName).CurrentValue;
            dbWriteContext.Entry(trackedPlayer).Property(fieldName).IsModified = true;
        }
        else
        {
            dbWriteContext.Items.Attach(item);
            dbWriteContext.Entry(item).Property(fieldName).IsModified = true;
        }
    }
}