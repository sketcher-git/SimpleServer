using SharedKernel;

namespace Domain.Players;

public static class PlayerErrors
{
    public static Error NotFound(Guid playerId) => Error.NotFound(
        "Players.NotFound", $"The player with the Id = '{playerId}' was not found");

    public static Error NotFoundByEmail(string email) => Error.NotFound(
        "Players.NotFoundByEmail", $"The player with the Email = '{email}' was not found");

    public static readonly Error EmailNotUnique = Error.Conflict(
        "Players.EmailNotUnique", "The provided email is not unique");

    public static readonly Error NameNotUnique = Error.Conflict(
        "Players.NameNotUnique", "The provided name is not unique");

    public static Error Existed(Guid playerId) => Error.Failure(
        "Players.Existed", $"The player with the Id = '{playerId}' already exists");
}