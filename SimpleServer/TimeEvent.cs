using SharedKernel;

namespace SimpleServer;

public class TimeEvent
{
    public TimeSpan Interval { get; set; }
    public DateTime NextTriggerTime { get; set; }
    public TimerNotification? Notification { get; set; }
    public bool IsRecurring { get; set; }
}