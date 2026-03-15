using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Robotico.Mediator;

namespace Robotico.Mediator.Benchmarks;

/// <summary>
/// Benchmarks for IMediator.SendAsync: no pipeline vs one pipeline behavior.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class MediatorSendBenchmarks
{
    private IServiceProvider _serviceProvider = null!;
    private IMediator _mediator = null!;
    private BenchQuery _query = null!;

    [GlobalSetup]
    public void Setup()
    {
        ServiceCollection services = new();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning));
        services.AddTransient<IMediator, Mediator>();
        services.AddTransient<IRequestHandler<BenchQuery, string>, BenchQueryHandler>();
        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
        _query = new BenchQuery(42);
    }

    [Benchmark(Baseline = true)]
    public async Task<string> SendAsync_NoPipeline()
    {
        return await _mediator.SendAsync(_query).ConfigureAwait(false);
    }

    [Benchmark]
    public async Task<string> SendAsync_WithOneBehavior()
    {
        return await _mediatorWithOneBehavior.SendAsync(_query).ConfigureAwait(false);
    }

    private IMediator _mediatorWithOneBehavior = null!;

    [IterationSetup(Target = nameof(SendAsync_WithOneBehavior))]
    public void SetupWithBehavior()
    {
        ServiceCollection services = new();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning));
        services.AddTransient<IMediator, Mediator>();
        services.AddTransient<IRequestHandler<BenchQuery, string>, BenchQueryHandler>();
        services.AddTransient<IPipelineBehavior<IRequest<string>, string>, NoOpBehavior>();
        _mediatorWithOneBehavior = services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    private sealed record BenchQuery(int Id) : IRequest<string>;

    private sealed class BenchQueryHandler : IRequestHandler<BenchQuery, string>
    {
        public Task<string> HandleAsync(BenchQuery request, CancellationToken cancellationToken = default) =>
            Task.FromResult($"id-{request.Id}");
    }

    private sealed class NoOpBehavior : IPipelineBehavior<IRequest<string>, string>
    {
        public async Task<string> HandleAsync(IRequest<string> request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken = default) =>
            await next().ConfigureAwait(false);
    }
}
