using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace loxxking_backend_clean.Application.Common.Behaviours;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Processing request {RequestName}", requestName);
        
        var timer = new Stopwatch();
        timer.Start();

        try
        {
            var response = await next();
            timer.Stop();
            _logger.LogInformation("Completed request {RequestName} in {ElapsedMilliseconds}ms", requestName, timer.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            timer.Stop();
            _logger.LogError(ex, "Request {RequestName} failed after {ElapsedMilliseconds}ms", requestName, timer.ElapsedMilliseconds);
            throw;
        }
    }
}
