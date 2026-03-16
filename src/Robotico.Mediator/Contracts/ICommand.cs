using System.Diagnostics.CodeAnalysis;

namespace Robotico.Mediator;

/// <summary>
/// Marker interface for commands that do not return data.
/// Commands represent intent to change the system state.
/// Handlers for this command type return <see cref="Robotico.Result.Result"/>.
/// </summary>
[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Marker interface for CQRS command contract; standard mediator pattern.")]
public interface ICommand : IRequest;
