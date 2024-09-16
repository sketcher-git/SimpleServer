namespace SharedKernel;

public interface ITimerService
{
    public bool RemoveEvent(Guid eventId);
    public Guid ScheduleEvent<T>(DateTime deadline, T message, bool isRecurring = false)
        where T : TimerNotification;
    public Guid ScheduleEvent<T>(TimeSpan interval, T message, bool isRecurring = false)
        where T : TimerNotification;
}