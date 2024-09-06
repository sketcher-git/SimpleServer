using SharedKernel;
using System.Text.RegularExpressions;

namespace Domain.Players;

public sealed record Email
{
    private const string _regular = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    private static readonly Regex _regex = new Regex(_regular);

    private Email(string value) => Value = value;

    public string Value { get; }

    public static Result<Email> Create(string? email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Result.Failure<Email>(EmailErrors.Empty, default);
        }

        if (!_regex.IsMatch(email))
        {
            return Result.Failure<Email>(EmailErrors.InvalidFormat, default);
        }

        return new Email(email);
    }
}