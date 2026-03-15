namespace Robotico.Mediator;

/// <summary>
/// Defines the contract for a mediator that dispatches requests to their corresponding handlers.
/// The mediator decouples the sender of a request from its handler, enabling a clean separation of concerns.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a request that returns a response of type <typeparamref name="TResponse"/> to its handler.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response returned by the handler.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation, containing the response from the handler.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no handler is registered for the request type.</exception>
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request that does not return data to its handler.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation, containing the result.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no handler is registered for the request type.</exception>
    Task<Robotico.Result.Result> SendAsync(IRequest request, CancellationToken cancellationToken = default);
}
