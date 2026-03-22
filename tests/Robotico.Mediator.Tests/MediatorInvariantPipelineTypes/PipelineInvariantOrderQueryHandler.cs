using Robotico.Mediator;

namespace Robotico.Mediator.Tests.MediatorInvariantPipelineTypes;

internal sealed class PipelineInvariantOrderQueryHandler : IRequestHandler<PipelineInvariantOrderQuery, int>
{
    /// <inheritdoc />
    public Task<int> HandleAsync(PipelineInvariantOrderQuery request, CancellationToken cancellationToken = default) =>
        Task.FromResult(request.Id);
}
