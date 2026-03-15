using Microsoft.Extensions.DependencyInjection;
using Robotico.Mediator;

namespace Robotico.Mediator.Tests;

/// <summary>
/// Tests for the mediator pipeline behavior chain, ordering, and short-circuiting.
/// </summary>
public class MediatorPipelineTests
{
    #region Test Types

    private record PipelineQuery(string Value) : IRequest<string>;

    private sealed class PipelineQueryHandler : IRequestHandler<PipelineQuery, string>
    {
        public Task<string> HandleAsync(PipelineQuery request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(request.Value);
        }
    }

    private sealed class FirstBehavior(List<string> executionOrder) : IPipelineBehavior<IRequest<string>, string>
    {
        public async Task<string> HandleAsync(IRequest<string> request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken = default)
        {
            executionOrder.Add("First:Before");
            string result = await next();
            executionOrder.Add("First:After");
            return result;
        }
    }

    private sealed class SecondBehavior(List<string> executionOrder) : IPipelineBehavior<IRequest<string>, string>
    {
        public async Task<string> HandleAsync(IRequest<string> request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken = default)
        {
            executionOrder.Add("Second:Before");
            string result = await next();
            executionOrder.Add("Second:After");
            return result;
        }
    }

    private sealed class ShortCircuitBehavior : IPipelineBehavior<IRequest<string>, string>
    {
        public Task<string> HandleAsync(IRequest<string> request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken = default)
        {
            return Task.FromResult("short-circuited");
        }
    }

    #endregion

    [Fact]
    public async Task Pipeline_ExecutesBehaviorsInRegistrationOrder()
    {
        List<string> executionOrder = [];
        ServiceCollection services = new();
        services.AddLogging();
        services.AddTransient<IRequestHandler<PipelineQuery, string>, PipelineQueryHandler>();
        services.AddTransient<IPipelineBehavior<IRequest<string>, string>>(_ => new FirstBehavior(executionOrder));
        services.AddTransient<IPipelineBehavior<IRequest<string>, string>>(_ => new SecondBehavior(executionOrder));
        services.AddTransient<IMediator, Robotico.Mediator.Mediator>();
        ServiceProvider provider = services.BuildServiceProvider();
        IMediator mediator = provider.GetRequiredService<IMediator>();
        PipelineQuery query = new("test");

        string result = await mediator.SendAsync(query);

        result.Should().Be("test");
        executionOrder.Should().ContainInOrder("First:Before", "Second:Before", "Second:After", "First:After");
    }

    [Fact]
    public async Task Pipeline_BehaviorCanShortCircuit()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddTransient<IRequestHandler<PipelineQuery, string>, PipelineQueryHandler>();
        services.AddTransient<IPipelineBehavior<IRequest<string>, string>, ShortCircuitBehavior>();
        services.AddTransient<IMediator, Robotico.Mediator.Mediator>();
        ServiceProvider provider = services.BuildServiceProvider();
        IMediator mediator = provider.GetRequiredService<IMediator>();
        PipelineQuery query = new("original");

        string result = await mediator.SendAsync(query);

        result.Should().Be("short-circuited");
    }
}
