namespace Robotico.Mediator;

/// <summary>
/// Represents a delegate that invokes the next behavior or the final handler in the pipeline.
/// </summary>
/// <typeparam name="TResponse">The type of response expected from the pipeline.</typeparam>
/// <returns>A task representing the asynchronous operation, containing the response.</returns>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();
