using Robotico.Mediator;

namespace Robotico.Mediator.Tests.MediatorErrorTestRecords;

/// <summary>
/// Void request type used only by <see cref="MediatorErrorTests"/> (no handler registered).
/// </summary>
internal sealed record UnhandledVoidCommandForErrorTests : IRequest;
