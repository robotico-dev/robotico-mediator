using Robotico.Mediator;

namespace Robotico.Mediator.Tests.MediatorInvariantPipelineTypes;

/// <summary>
/// Request type used only by <see cref="MediatorInvariantTests"/> pipeline-order tests.
/// </summary>
internal sealed record PipelineInvariantOrderQuery(int Id) : IRequest<int>;
