using Robotico.Mediator;

namespace Robotico.Mediator.Tests.MediatorErrorTestRecords;

/// <summary>
/// Request type used only by <see cref="MediatorErrorTests"/> (no handler registered).
/// </summary>
internal sealed record UnhandledQueryForErrorTests(string Value) : IRequest<string>;
