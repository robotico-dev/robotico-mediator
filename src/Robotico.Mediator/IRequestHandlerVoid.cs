namespace Robotico.Mediator;

/// <summary>
/// Defines a handler for a request that returns <see cref="Robotico.Result.Result"/> with no data.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
public interface IRequestHandler<in TRequest>
    where TRequest : IRequest
{
    /// <summary>
    /// Handles the specified request asynchronously.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation, containing the result.</returns>
    Task<Robotico.Result.Result> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
