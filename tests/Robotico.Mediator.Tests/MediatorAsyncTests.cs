using Microsoft.Extensions.DependencyInjection;
using Robotico.Mediator;
using VoidResult = Robotico.Result.Result;

namespace Robotico.Mediator.Tests;

/// <summary>
/// Tests for async handler execution in the mediator.
/// </summary>
public class MediatorAsyncTests
{
    #region Test Types

    private record AsyncQuery(int DelayMs) : IRequest<string>;

    private sealed class AsyncQueryHandler : IRequestHandler<AsyncQuery, string>
    {
        public async Task<string> HandleAsync(AsyncQuery request, CancellationToken cancellationToken = default)
        {
            await Task.Delay(request.DelayMs, cancellationToken);
            return "completed";
        }
    }

    private record AsyncVoidCommand(int DelayMs) : IRequest;

    private sealed class AsyncVoidCommandHandler : IRequestHandler<AsyncVoidCommand>
    {
        public async Task<VoidResult> HandleAsync(AsyncVoidCommand request, CancellationToken cancellationToken = default)
        {
            await Task.Delay(request.DelayMs, cancellationToken);
            return VoidResult.Success();
        }
    }

    #endregion

    private static IMediator CreateMediator()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddTransient<IMediator, Robotico.Mediator.Mediator>();
        services.AddTransient<IRequestHandler<AsyncQuery, string>, AsyncQueryHandler>();
        services.AddTransient<IRequestHandler<AsyncVoidCommand>, AsyncVoidCommandHandler>();
        ServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task SendAsync_AsyncHandler_CompletesSuccessfully()
    {
        IMediator mediator = CreateMediator();
        AsyncQuery query = new(10);

        string result = await mediator.SendAsync(query);

        result.Should().Be("completed");
    }

    [Fact]
    public async Task SendAsync_AsyncVoidHandler_CompletesSuccessfully()
    {
        IMediator mediator = CreateMediator();
        AsyncVoidCommand command = new(10);

        VoidResult result = await mediator.SendAsync(command);

        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        IMediator mediator = CreateMediator();
        AsyncQuery query = new(5000);
        CancellationTokenSource cts = new();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => mediator.SendAsync(query, cts.Token));
    }
}
