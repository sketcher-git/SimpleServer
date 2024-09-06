using MediatR;
using SharedKernel;

namespace Application.Chat;

public sealed record ChatNotification(Guid SenderId, ChatType Channel, Guid TargetId, string Content, DateTime SendingTime)
    : INotification;