using Microsoft.Extensions.DependencyInjection;
using Robotico.Mediator;

namespace Robotico.Mediator.Tests;

/// <summary>
/// Tests for mediator error handling: missing handler, null request.
/// </summary>
public class MediatorErrorTests
{
    #region Test Types

    private record UnhandledQuery(string Value) : IRequest<string>;

    private record UnhandledVoidCommand : IRequest;

    #endregion

    private sealed class MediatorScope : IDisposable
    {
        private readonly ServiceProvider _provider;
        public IMediator Mediator { get; }
        public MediatorScope(ServiceProvider provider, IMediator mediator) { _provider = provider; Mediator = mediator; }
        public void Dispose() => _provider.Dispose();
    }

    private static MediatorScope CreateEmptyMediator()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddTransient<IMediator, Robotico.Mediator.Mediator>();
        ServiceProvider provider = services.BuildServiceProvider();
        return new MediatorScope(provider, provider.GetRequiredService<IMediator>());
    }

    [Fact]
    public async Task SendAsync_WithNoRegisteredHandler_ThrowsInvalidOperationException()
    {
        using MediatorScope scope = CreateEmptyMediator();
        IMediator mediator = scope.Mediator;
        UnhandledQuery query = new("test");

        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.SendAsync(query));
    }

    [Fact]
    public async Task SendAsync_VoidWithNoRegisteredHandler_ThrowsInvalidOperationException()
    {
        using MediatorScope scope = CreateEmptyMediator();
        IMediator mediator = scope.Mediator;
        UnhandledVoidCommand command = new();

        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.SendAsync(command));
    }

    [Fact]
    public async Task SendAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        using MediatorScope scope = CreateEmptyMediator();
        IMediator mediator = scope.Mediator;

        await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.SendAsync<string>(null!));
    }

    [Fact]
    public async Task SendAsync_VoidWithNullRequest_ThrowsArgumentNullException()
    {
        using MediatorScope scope = CreateEmptyMediator();
        IMediator mediator = scope.Mediator;

        await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.SendAsync(null!));
    }

    [Fact]
    public async Task SendAsync_WithMultipleHandlersForSameRequest_ThrowsInvalidOperationException()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddTransient<IMediator, Robotico.Mediator.Mediator>();
        services.AddTransient<IRequestHandler<UnhandledQuery, string>, DuplicateHandlerOne>();
        services.AddTransient<IRequestHandler<UnhandledQuery, string>, DuplicateHandlerTwo>();
        using ServiceProvider provider = services.BuildServiceProvider();
        IMediator mediator = provider.GetRequiredService<IMediator>();
        UnhandledQuery query = new("test");

        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.SendAsync(query));
    }

    private sealed class DuplicateHandlerOne : IRequestHandler<UnhandledQuery, string>
    {
        public Task<string> HandleAsync(UnhandledQuery request, CancellationToken cancellationToken = default) =>
            Task.FromResult("one");
    }

    private sealed class DuplicateHandlerTwo : IRequestHandler<UnhandledQuery, string>
    {
        public Task<string> HandleAsync(UnhandledQuery request, CancellationToken cancellationToken = default) =>
            Task.FromResult("two");
    }
}
