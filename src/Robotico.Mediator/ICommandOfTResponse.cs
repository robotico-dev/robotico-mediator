namespace Robotico.Mediator;

/// <summary>
/// Marker interface for commands that return a response of type <typeparamref name="TResponse"/>.
/// Commands represent intent to change the system state.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned by the handler.</typeparam>
public interface ICommand<TResponse> : IRequest<TResponse>;
