namespace Robotico.Mediator;

/// <summary>
/// Marker interface for requests that do not return data.
/// Handlers for this request type return <see cref="Robotico.Result.Result"/>.
/// Extends IRequest of Result so void requests participate in the same pipeline as typed requests.
/// </summary>
public interface IRequest : IRequest<Robotico.Result.Result>;
