namespace Robotico.Mediator;

/// <summary>
/// Defines a pipeline behavior that wraps the handling of a request.
/// Pipeline behaviors execute in registration order before the handler is invoked.
/// They can perform cross-cutting concerns such as logging, validation, or transaction management.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
/// <remarks>
/// Pipeline behaviors form a chain. Each behavior receives the request and a delegate to the next behavior
/// (or the final handler). A behavior can short-circuit the pipeline by returning a response without
/// calling the next delegate.
/// </remarks>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the specified request within the pipeline.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="next">A delegate to the next behavior or the final handler in the pipeline.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation, containing the response.</returns>
    Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default);
}
