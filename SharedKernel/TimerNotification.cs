using MediatR;

namespace SharedKernel;

public abstract record TimerNotification : INotification
{
    public Guid EventId { get; set; }
    public DateTime TriggerTime { get; set; }
}