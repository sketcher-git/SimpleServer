namespace Application.Abstractions.Services;

public interface IDateTimeProvider
{
    public DateTime UtcNow { get; }
}