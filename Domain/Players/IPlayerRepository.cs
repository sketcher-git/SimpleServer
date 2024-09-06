namespace Domain.Players;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> IsEmailUniqueAsync(Email email);

    Task<bool> IsNameUniqueAsync(Name email);

    void Insert(Player player);

    void Update(Player player, string fieldName);
}