using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Robotico.Mediator;

/// <summary>
/// Default implementation of <see cref="IMediator"/> that resolves handlers and pipeline behaviors
/// from <see cref="IServiceProvider"/> and dispatches requests through the pipeline.
/// </summary>
/// <remarks>
/// The mediator resolves a single handler per request type. If no handler is found, a
/// <see cref="MediatorNoHandlerException"/> is thrown. Pipeline behaviors are resolved and executed
/// in registration order before the handler is invoked. Void requests (<see cref="IRequest"/>) use the same
/// pipeline as typed requests returning <see cref="Robotico.Result.Result"/>.
/// Handler interface type and <see cref="MethodInfo"/> are cached per (request type, response type) to avoid
/// reflection on the hot path.
/// </remarks>
[SuppressMessage("Naming", "CA1724:Type names should not match namespaces", Justification = "Mediator is the standard name for this pattern; namespace Robotico.Mediator is the package identity.")]
public sealed class Mediator(IServiceProvider serviceProvider, ILogger<Mediator> logger) : IMediator
{
    private static readonly ConcurrentDictionary<(Type RequestType, Type ResponseType), (Type HandlerInterfaceType, MethodInfo HandleMethod)> HandlerCache = new();
    private static readonly ConcurrentDictionary<Type, (Type HandlerInterfaceType, MethodInfo HandleMethod)> VoidHandlerCache = new();

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    /// <exception cref="MediatorNoHandlerException">Thrown when no handler is registered for the request type.</exception>
    /// <exception cref="InvalidOperationException">Thrown when multiple handlers are registered for the same request type.</exception>
    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Type requestType = request.GetType();
        Type responseType = typeof(TResponse);
        (Type handlerInterfaceType, MethodInfo handleMethod) = GetOrAddHandlerInfo(requestType, responseType);

        logger.LogDebug(MediatorEventIds.RequestStarted, "Dispatching request of type {RequestType}", requestType.Name);

        object? handler = ResolveSingleHandler(handlerInterfaceType, requestType);
        if (handler is null && responseType == typeof(Robotico.Result.Result))
        {
            (Type voidInterfaceType, MethodInfo voidHandleMethod) = GetOrAddVoidHandlerInfo(requestType);
            handler = ResolveSingleHandler(voidInterfaceType, requestType);
            if (handler is not null)
            {
                handlerInterfaceType = voidInterfaceType;
                handleMethod = voidHandleMethod;
            }
        }

        if (handler is null)
        {
            throw new MediatorNoHandlerException(requestType.Name);
        }

        IEnumerable<IPipelineBehavior<IRequest<TResponse>, TResponse>> behaviors =
            serviceProvider.GetServices<IPipelineBehavior<IRequest<TResponse>, TResponse>>();

        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
        {
            Task<TResponse> task = (Task<TResponse>)handleMethod.Invoke(handler, [request, cancellationToken])!;
            return task;
        };

        RequestHandlerDelegate<TResponse> pipeline = behaviors
            .Reverse()
            .Aggregate(handlerDelegate, (next, behavior) => () => behavior.HandleAsync((IRequest<TResponse>)request, next, cancellationToken));

        Stopwatch sw = Stopwatch.StartNew();
        try
        {
            TResponse response = await pipeline().ConfigureAwait(false);
            sw.Stop();
            logger.LogDebug(MediatorEventIds.RequestCompleted, "Request {RequestType} completed in {DurationMs}ms", requestType.Name, sw.ElapsedMilliseconds);
            if (typeof(TResponse) == typeof(Robotico.Result.Result))
            {
                Robotico.Result.Result result = (Robotico.Result.Result)(object)response!;
                logger.LogDebug(MediatorEventIds.RequestCompleted, "Request {RequestType} result: Success={Success}", requestType.Name, result.IsSuccess());
            }
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            Exception toRethrow = ex is System.Reflection.TargetInvocationException tie && tie.InnerException is not null ? tie.InnerException : ex;
            logger.LogWarning(MediatorEventIds.RequestFailed, toRethrow, "Request {RequestType} failed after {DurationMs}ms", requestType.Name, sw.ElapsedMilliseconds);
            throw toRethrow;
        }
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    /// <exception cref="MediatorNoHandlerException">Thrown when no handler is registered for the request type.</exception>
    /// <exception cref="InvalidOperationException">Thrown when multiple handlers are registered for the same request type.</exception>
    public Task<Robotico.Result.Result> SendAsync(IRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendAsync<Robotico.Result.Result>((IRequest<Robotico.Result.Result>)request, cancellationToken);
    }

    private static (Type HandlerInterfaceType, MethodInfo HandleMethod) GetOrAddHandlerInfo(Type requestType, Type responseType)
    {
        (Type RequestType, Type ResponseType) key = (requestType, responseType);
        if (HandlerCache.TryGetValue(key, out (Type HandlerInterfaceType, MethodInfo HandleMethod) cached))
        {
            return cached;
        }

        Type handlerInterfaceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        MethodInfo? handleMethod = handlerInterfaceType.GetMethod(
            nameof(IRequestHandler<IRequest<object>, object>.HandleAsync),
            [requestType, typeof(CancellationToken)]);
        if (handleMethod is null)
        {
            throw new InvalidOperationException(
                $"No handler interface method found for request type {requestType.Name} and response type {responseType.Name}.");
        }

        cached = (handlerInterfaceType, handleMethod);
        HandlerCache.TryAdd(key, cached);
        return cached;
    }

    private static (Type HandlerInterfaceType, MethodInfo HandleMethod) GetOrAddVoidHandlerInfo(Type requestType)
    {
        if (VoidHandlerCache.TryGetValue(requestType, out (Type HandlerInterfaceType, MethodInfo HandleMethod) cached))
        {
            return cached;
        }

        Type handlerInterfaceType = typeof(IRequestHandler<>).MakeGenericType(requestType);
        MethodInfo? handleMethod = handlerInterfaceType.GetMethod(
            "HandleAsync",
            [requestType, typeof(CancellationToken)]);
        if (handleMethod is null)
        {
            throw new InvalidOperationException(
                $"No void handler interface method found for request type {requestType.Name}.");
        }

        cached = (handlerInterfaceType, handleMethod);
        VoidHandlerCache.TryAdd(requestType, cached);
        return cached;
    }

    private object? ResolveSingleHandler(Type handlerInterfaceType, Type requestType)
    {
        object?[] handlers = (from object? h in serviceProvider.GetServices(handlerInterfaceType) select h).ToArray();
        if (handlers.Length > 1)
        {
            throw new InvalidOperationException(
                $"Multiple handlers registered for request type {requestType.Name}. Register only one handler per request type.");
        }
        return handlers.Length == 1 ? handlers[0] : null;
    }

}
