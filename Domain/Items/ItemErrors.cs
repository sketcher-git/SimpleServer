using SharedKernel;

namespace Domain.Items;

public class ItemErrors
{
    public static Error Insufficient(Guid itemId) => Error.Failure(
        "Items.Insufficient", $"The item with the Id = '{itemId}' was insufficient"
    );

    public static Error Insufficient(int dataId) => Error.Failure(
        "Items.Insufficient", $"The item with the data Id = '{dataId}' was insufficient"
    );

    public static Error InvalidAmount(Guid playerId, int amount) => Error.Failure(
        "Items.InvalidAmount", $"The player with the Id = '{playerId}' attampted to buy {amount} items"
    );

    public static Error InvalidQuery(Guid playerId) => Error.Validation(
        "Items.Unloaded", $"The query about <item records of player with the Id = '{playerId}'> was invalid. The player had to login to the server first"
    );

    public static Error NonExistent(Guid itemId) => Error.NotFound(
        "Items.NonExistent", $"The item with the Id = '{itemId}' was non-existent"
    );

    public static Error NonExistent(int dataId) => Error.NotFound(
        "Items.NonExistent", $"The item with the Data Id = '{dataId}' was non-existent"
    );

    public static Error Unloaded(Guid playerId) => Error.Validation(
        "Items.Unloaded", $"The item records of player with the Id = '{playerId}' was unloaded"
    );
}