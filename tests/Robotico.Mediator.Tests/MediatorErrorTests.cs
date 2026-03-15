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

    private static IMediator CreateEmptyMediator()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddTransient<IMediator, Robotico.Mediator.Mediator>();
        ServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task SendAsync_WithNoRegisteredHandler_ThrowsInvalidOperationException()
    {
        IMediator mediator = CreateEmptyMediator();
        UnhandledQuery query = new("test");

        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.SendAsync(query));
    }

    [Fact]
    public async Task SendAsync_VoidWithNoRegisteredHandler_ThrowsInvalidOperationException()
    {
        IMediator mediator = CreateEmptyMediator();
        UnhandledVoidCommand command = new();

        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.SendAsync(command));
    }

    [Fact]
    public async Task SendAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        IMediator mediator = CreateEmptyMediator();

        await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.SendAsync<string>(null!));
    }

    [Fact]
    public async Task SendAsync_VoidWithNullRequest_ThrowsArgumentNullException()
    {
        IMediator mediator = CreateEmptyMediator();

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
        IMediator mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
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
