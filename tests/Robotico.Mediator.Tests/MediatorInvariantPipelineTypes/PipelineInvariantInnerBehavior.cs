using Robotico.Mediator;

namespace Robotico.Mediator.Tests.MediatorInvariantPipelineTypes;

internal sealed class PipelineInvariantInnerBehavior(List<string> order) : IPipelineBehavior<IRequest<int>, int>
{
    /// <inheritdoc />
    public async Task<int> HandleAsync(IRequest<int> request, RequestHandlerDelegate<int> next, CancellationToken cancellationToken = default)
    {
        order.Add("Inner:Before");
        int result = await next();
        order.Add("Inner:After");
        return result;
    }
}
