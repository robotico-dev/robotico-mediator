namespace Robotico.Mediator;

/// <summary>
/// Marker interface for commands that do not return data.
/// Commands represent intent to change the system state.
/// Handlers for this command type return <see cref="Robotico.Result.Result"/>.
/// </summary>
public interface ICommand : IRequest;
