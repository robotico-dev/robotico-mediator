namespace Robotico.Mediator;

/// <summary>
/// Marker interface for requests that return a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned by the handler.</typeparam>
public interface IRequest<TResponse>;
