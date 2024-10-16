﻿using MediatR;
using SharedKernel;

namespace Application.Chat;

public sealed record ChatNotification(Guid SenderId, ChatType Channel, Guid TargetId, string SenderName, string Content, DateTime SendingTime)
    : INotification;