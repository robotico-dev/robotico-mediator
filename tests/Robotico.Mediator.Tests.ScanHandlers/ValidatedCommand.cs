using Robotico.Mediator;

namespace Robotico.Mediator.Tests.ScanHandlers;

/// <summary>
/// Request used with <see cref="ValidatedCommandValidator"/> in validation pipeline tests.
/// </summary>
public record ValidatedCommand(string Name, int Age) : IRequest;
