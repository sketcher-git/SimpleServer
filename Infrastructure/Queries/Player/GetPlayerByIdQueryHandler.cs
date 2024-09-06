using Application.Abstractions.Messaging;
using Application.Data;
using Application.Players.GetById;
using Domain.Players;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Protocols;
using SharedKernel;

namespace Infrastructure.Queries.Players;

internal sealed class GetPlayerByIdQueryHandler(ApplicationReadDbContext dbContext)
    : IQueryHandler<GetPlayerByIdQuery, PlayerInformationResponseProtocol>
{
    public async Task<Result<PlayerInformationResponseProtocol>> Handle(GetPlayerByIdQuery query, CancellationToken cancellationToken)
    {
        var response = new PlayerInformationResponseProtocol();
        var targetId = query.TargetPlayerId;
        var record = OnlineCacheController.Instance.GetPlayerRecordByPlayerId(targetId);
        if (record == null)
            record = await dbContext.Players
                .Where(p => p.Id == query.PlayerId)
                .FirstOrDefaultAsync(cancellationToken);

        if (record is null)
        {
            response.ErrorType = ErrorType.NotFound;
            return Result.Failure(PlayerErrors.NotFound(targetId), response);
        }

        response.ErrorType = ErrorType.None;
        response.PlayerInfo = (record.Id, record.Name.Value, record.LoginTimestamp);
        return response;
    }
}