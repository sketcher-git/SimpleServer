namespace SharedKernel;

public interface ITimerService
{
    public void RemoveEvent(Guid eventId);
    public void ScheduleEvent<T>(DateTime deadline, T message, bool isRecurring = false)
        where T : TimerNotification;
    public void ScheduleEvent<T>(TimeSpan interval, T message, bool isRecurring = false)
        where T : TimerNotification;
}