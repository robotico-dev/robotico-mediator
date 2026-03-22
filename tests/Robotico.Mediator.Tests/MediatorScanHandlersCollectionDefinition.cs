using Xunit;

namespace Robotico.Mediator.Tests;

/// <summary>
/// Serializes tests that touch <see cref="Robotico.Mediator.Tests.ScanHandlers.ValidatedCommandHandler.Invoked"/> (static) so parallel runs do not flake.
/// </summary>
[CollectionDefinition("MediatorScanHandlers", DisableParallelization = true)]
public sealed class MediatorScanHandlersCollectionDefinition
{
}
