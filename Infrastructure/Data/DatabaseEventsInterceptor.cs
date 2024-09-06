using Application.Abstractions.Services;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SharedKernel;

namespace Infrastructure.Data;

internal sealed class DatabaseEventsInterceptor(IServiceApi api) : SaveChangesInterceptor
{
    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        base.SaveChangesFailed(eventData);
        api.WriteLog(LogLevelType.Error, eventData.Exception.Message);
    }
}