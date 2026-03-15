namespace Robotico.Mediator;

/// <summary>
/// Defines a handler for a command that returns <see cref="Robotico.Result.Result"/> with no data.
/// </summary>
/// <typeparam name="TCommand">The type of command being handled.</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand;
