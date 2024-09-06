using MediatR;
using SharedKernel;

namespace Application.Players.Login;

public sealed record LoginNotification(Guid PlayerId, string Name) : INotification;