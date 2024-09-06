using SharedKernel;

namespace Application.Timer;

public sealed record LogOutDelayNotification(Guid playerId) : TimerNotification;