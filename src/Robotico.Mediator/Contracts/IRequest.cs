using System.Diagnostics.CodeAnalysis;

namespace Robotico.Mediator;

/// <summary>
/// Marker interface for requests that return a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned by the handler.</typeparam>
[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Marker interface for request/response contract; standard mediator pattern.")]
public interface IRequest<TResponse>;
