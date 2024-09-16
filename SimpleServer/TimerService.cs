using Application.Abstractions.Services;
using MediatR;
using SharedKernel;
using System.Collections.Concurrent;

namespace SimpleServer;

public sealed class TimerService : ITimerService
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMediator _mediator;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly ConcurrentDictionary<Guid, TimeEvent> _events = new ConcurrentDictionary<Guid, TimeEvent>();

    public TimerService(IDateTimeProvider dateTimeProvider, IMediator mediator)
    {
        _dateTimeProvider = dateTimeProvider;
        _mediator = mediator;
        Task.Run(() => EventLoopAsync(_cts.Token));
    }

    private async Task EventLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            DateTime now = _dateTimeProvider.UtcNow;

            foreach (var kvp in _events)
            {
                var timeEvent = kvp.Value;

                if (timeEvent.NextTriggerTime <= now)
                {
                    var notification = timeEvent.Notification;
                    notification.TriggerTime = now;
                    await _mediator.Publish(notification);

                    if (timeEvent.IsRecurring)
                        timeEvent.NextTriggerTime = now + timeEvent.Interval;
                    else
                        _events.TryRemove(kvp.Key, out _);
                }
            }

            await Task.Delay(1000, token);
        }
    }

    public bool RemoveEvent(Guid eventId)
    {
        return _events.TryRemove(eventId, out _);
    }

    public Guid ScheduleEvent<T>(DateTime deadline, T notification, bool isRecurring = false)
        where T : TimerNotification
    {
        if (deadline <= _dateTimeProvider.UtcNow)
            return Guid.Empty;
        return ScheduleEvent(deadline - _dateTimeProvider.UtcNow, notification, isRecurring);
    }

    public Guid ScheduleEvent<T>(TimeSpan interval, T notification, bool isRecurring = false)
        where T : TimerNotification
    {
        var timeEvent = new TimeEvent
        {
            Interval = interval,
            Notification = notification,
            IsRecurring = isRecurring,
            NextTriggerTime = _dateTimeProvider.UtcNow + interval
        };

        Guid eventId = Guid.NewGuid();
        _events[eventId] = timeEvent;
        notification.EventId = eventId;
        return eventId;
    }

    internal void StopService()
    {
        _cts.Cancel();
    }
}