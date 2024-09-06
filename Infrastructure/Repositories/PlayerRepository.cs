using Domain.Players;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class PlayerRepository(ApplicationReadDbContext dbReadContext, ApplicationWriteDbContext dbWriteContext) : IPlayerRepository
{
    public async Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbReadContext.Players.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(Email email)
    {
        return !await dbReadContext.Players.AnyAsync(p => p.Email == email);
    }

    public async Task<bool> IsNameUniqueAsync(Name name)
    {
        return !await dbReadContext.Players.AnyAsync(p => p.Name == name);
    }

    public void Insert(Player player)
    {
        dbWriteContext.Players.Add(player);
    }

    public void Update(Player player, string fieldName)
    {
        var trackedPlayer = dbWriteContext.Players.Local.FirstOrDefault(p => p.Id == player.Id);
        if (trackedPlayer != null)
        {
            dbWriteContext.Entry(trackedPlayer).Property(fieldName).CurrentValue =
                dbWriteContext.Entry(player).Property(fieldName).CurrentValue;
            dbWriteContext.Entry(trackedPlayer).Property(fieldName).IsModified = true;
        }
        else
        {
            dbWriteContext.Players.Attach(player);
            dbWriteContext.Entry(player).Property(fieldName).IsModified = true;
        }
    }
}