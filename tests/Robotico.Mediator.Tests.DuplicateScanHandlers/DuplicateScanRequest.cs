using Robotico.Mediator;

namespace Robotico.Mediator.Tests.DuplicateScanHandlers;

/// <summary>
/// Request type shared by duplicate handlers (intentional) for AddMediator negative tests.
/// </summary>
public record DuplicateScanRequest(int Id) : IRequest<string>;
