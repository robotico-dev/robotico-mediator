namespace Robotico.Mediator;

/// <summary>
/// Marker interface for queries that return a response of type <typeparamref name="TResponse"/>.
/// Queries represent a request to read data without side effects.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned by the handler.</typeparam>
public interface IQuery<TResponse> : IRequest<TResponse>;
