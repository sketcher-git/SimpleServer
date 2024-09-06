using Application.Abstractions.Services;
using MediatR;
using Serilog.Context;
using SharedKernel;

namespace Application.Abstractions.Behaviors;

internal sealed class RequestLoggingPipelineBehavior<TRequest, TResponse>(IServiceApi api)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;

#if DEBUG
        api.WriteLog(LogLevelType.Notice, $"Processing request {requestName}");
#endif

        TResponse result = await next();

        if (result.IsSuccess)
        {
#if DEBUG
            api.WriteLog(LogLevelType.Notice, $"Completed request {requestName}");
#endif
        }
        else
        {
            using (LogContext.PushProperty("Error", result.Error, true))
            {
                api.WriteLog(LogLevelType.Error, $"Completed request {requestName} with error: {result.Error.Description}");
            }
        }

        return result;
    }
}