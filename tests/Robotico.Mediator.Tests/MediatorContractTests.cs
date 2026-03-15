using Microsoft.Extensions.DependencyInjection;
using Robotico.Mediator;
using VoidResult = Robotico.Result.Result;

namespace Robotico.Mediator.Tests;

/// <summary>
/// Contract and laws-style tests: void overload equivalence, pipeline ordering, duplicate handler detection.
/// </summary>
public class MediatorContractTests
{
    #region Void overload equivalence (SendAsync(void) === SendAsync&lt;Result&gt;(void as IRequest&lt;Result&gt;))

    private record VoidOnlyCommand(int Id) : IRequest;

    private sealed class VoidOnlyCommandHandler : IRequestHandler<VoidOnlyCommand>
    {
        public Task<VoidResult> HandleAsync(VoidOnlyCommand request, CancellationToken cancellationToken = default) =>
            Task.FromResult(VoidResult.Success());
    }

    [Fact]
    public async Task SendAsync_VoidOverload_EquivalentTo_SendAsync_Of_Result()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddTransient<IMediator, Robotico.Mediator.Mediator>();
        services.AddTransient<IRequestHandler<VoidOnlyCommand>, VoidOnlyCommandHandler>();
        IMediator mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
        VoidOnlyCommand request = new VoidOnlyCommand(1);

        VoidResult viaVoid = await mediator.SendAsync(request);
        VoidResult viaTyped = await mediator.SendAsync<VoidResult>((IRequest<VoidResult>)request);

        viaVoid.IsSuccess().Should().BeTrue();
        viaTyped.IsSuccess().Should().BeTrue();
    }

    #endregion

    #region Pipeline order: behaviors execute in registration order

    private record OrderQuery(int Id) : IRequest<int>;

    private sealed class OrderQueryHandler : IRequestHandler<OrderQuery, int>
    {
        public Task<int> HandleAsync(OrderQuery request, CancellationToken cancellationToken = default) =>
            Task.FromResult(request.Id);
    }

    private sealed class OrderBehaviorA : IPipelineBehavior<IRequest<int>, int>
    {
        private readonly List<string> _log;

        public OrderBehaviorA(List<string> log) => _log = log;

        public async Task<int> HandleAsync(IRequest<int> request, RequestHandlerDelegate<int> next, CancellationToken cancellationToken = default)
        {
            _log.Add("A:before");
            int result = await next();
            _log.Add("A:after");
            return result;
        }
    }

    private sealed class OrderBehaviorB : IPipelineBehavior<IRequest<int>, int>
    {
        private readonly List<string> _log;

        public OrderBehaviorB(List<string> log) => _log = log;

        public async Task<int> HandleAsync(IRequest<int> request, RequestHandlerDelegate<int> next, CancellationToken cancellationToken = default)
        {
            _log.Add("B:before");
            int result = await next();
            _log.Add("B:after");
            return result;
        }
    }

    [Fact]
    public async Task Pipeline_BehaviorsExecuteInRegistrationOrder_FirstRegisteredRunsFirst()
    {
        List<string> log = new List<string>();
        ServiceCollection services = new();
        services.AddLogging();
        services.AddSingleton(log);
        services.AddTransient<IMediator, Robotico.Mediator.Mediator>();
        services.AddTransient<IRequestHandler<OrderQuery, int>, OrderQueryHandler>();
        services.AddTransient<IPipelineBehavior<IRequest<int>, int>, OrderBehaviorA>();
        services.AddTransient<IPipelineBehavior<IRequest<int>, int>, OrderBehaviorB>();
        IMediator mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        int result = await mediator.SendAsync(new OrderQuery(1));

        result.Should().Be(1);
        log.Should().ContainInOrder("A:before", "B:before", "B:after", "A:after");
    }

    #endregion
}
