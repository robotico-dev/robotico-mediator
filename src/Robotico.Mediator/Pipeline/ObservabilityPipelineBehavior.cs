using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Robotico.Mediator;

/// <summary>
/// Pipeline behavior that adds observability: distributed tracing (Activity), structured logging (request type, duration, result),
/// and optional metrics. Register this behavior so it runs first (outermost) to wrap the entire pipeline.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type (e.g. <see cref="Robotico.Result.Result"/> for commands).</typeparam>
/// <remarks>
/// When <typeparamref name="TResponse"/> is <see cref="Robotico.Result.Result"/>, logs success vs failure without leaking domain data.
/// Uses a static <see cref="ActivitySource"/> named "Robotico.Mediator" for OpenTelemetry / Application Insights correlation.
/// </remarks>
public sealed class ObservabilityPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly ActivitySource MediatorActivitySource = new("Robotico.Mediator", "1.0.0");
    private readonly ILogger _logger;

    /// <summary>
    /// Creates an observability behavior with the given logger factory.
    /// </summary>
    /// <param name="loggerFactory">Used to create a logger with category "Robotico.Mediator.Observability".</param>
    public ObservabilityPipelineBehavior(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger("Robotico.Mediator.Observability");
    }

    /// <inheritdoc />
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        string requestTypeName = request.GetType().Name;
        using Activity? activity = MediatorActivitySource.StartActivity(requestTypeName);
        activity?.SetTag("request.type", requestTypeName);

        _logger.LogDebug(MediatorEventIds.ObservabilityRequestStarted, "Request {RequestType} started", requestTypeName);

        Stopwatch sw = Stopwatch.StartNew();
        try
        {
            TResponse response = await next().ConfigureAwait(false);
            sw.Stop();

            activity?.SetTag("duration_ms", sw.ElapsedMilliseconds);

            if (typeof(TResponse) == typeof(Robotico.Result.Result))
            {
                Robotico.Result.Result result = (Robotico.Result.Result)(object)response!;
                bool success = result.IsSuccess();
                activity?.SetTag("result.success", success);
                _logger.LogDebug(MediatorEventIds.ObservabilityRequestCompleted, "Request {RequestType} completed in {DurationMs}ms, Success={Success}", requestTypeName, sw.ElapsedMilliseconds, success);
            }
            else
            {
                _logger.LogDebug(MediatorEventIds.ObservabilityRequestCompleted, "Request {RequestType} completed in {DurationMs}ms", requestTypeName, sw.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            activity?.SetTag("error", true);
            activity?.SetTag("exception.message", ex.Message);
            _logger.LogWarning(MediatorEventIds.ObservabilityRequestFailed, ex, "Request {RequestType} failed after {DurationMs}ms", requestTypeName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
