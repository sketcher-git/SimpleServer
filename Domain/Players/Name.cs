using SharedKernel;
using System.Text.RegularExpressions;

namespace Domain.Players;

public sealed record Name
{
    private const string _regular = @"^(?:[a-zA-Z][a-zA-Z0-9]{1,11}|[\u4e00-\u9fa5]{2,6})$";
    private static readonly Regex _regex = new Regex(_regular);

    private Name(string value) => Value = value;

    public string Value { get; }

    public static Result<Name> Create(string? name)
    {
        if (!_regex.IsMatch(name))
        {
            return Result.Failure<Name>(NameErrors.InvalidFormat, default);
        }

        return new Name(name);
    }
}