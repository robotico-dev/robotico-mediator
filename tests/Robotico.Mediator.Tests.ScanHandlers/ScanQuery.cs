using Robotico.Mediator;

namespace Robotico.Mediator.Tests.ScanHandlers;

/// <summary>
/// Request type for AddMediator assembly-scan tests (isolated assembly; see sibling types).
/// </summary>
public record ScanQuery(string Value) : IRequest<string>;
