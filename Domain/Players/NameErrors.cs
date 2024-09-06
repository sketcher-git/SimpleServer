using SharedKernel;

namespace Domain.Players;

public static class NameErrors
{
    public static readonly Error InvalidFormat = Error.Validation(
        "Name.InvalidFormat", "Name format is invalid");
}