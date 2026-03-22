using Microsoft.Extensions.DependencyInjection;
using Robotico.Mediator;
using Robotico.Mediator.Tests.MediatorErrorTestHandlers;
using Robotico.Mediator.Tests.MediatorErrorTestRecords;
using Xunit;

namespace Robotico.Mediator.Tests;

/// <summary>
/// Tests for mediator error handling: missing handler, null request.
/// </summary>
public sealed class MediatorErrorTests
{
    private static MediatorErrorTestScope CreateEmptyMediator()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddTransient<IMediator, Robotico.Mediator.Mediator>();
        ServiceProvider provider = services.BuildServiceProvider();
        return new MediatorErrorTestScope(provider, provider.GetRequiredService<IMediator>());
    }

    [Fact]
    public async Task SendAsync_WithNoRegisteredHandler_ThrowsMediatorNoHandlerException()
    {
        using MediatorErrorTestScope scope = CreateEmptyMediator();
        IMediator mediator = scope.Mediator;
        UnhandledQueryForErrorTests query = new("test");

        await Assert.ThrowsAsync<MediatorNoHandlerException>(() => mediator.SendAsync(query));
    }

    [Fact]
    public async Task SendAsync_VoidWithNoRegisteredHandler_ThrowsMediatorNoHandlerException()
    {
        using MediatorErrorTestScope scope = CreateEmptyMediator();
        IMediator mediator = scope.Mediator;
        UnhandledVoidCommandForErrorTests command = new();

        await Assert.ThrowsAsync<MediatorNoHandlerException>(() => mediator.SendAsync(command));
    }

    [Fact]
    public async Task SendAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        using MediatorErrorTestScope scope = CreateEmptyMediator();
        IMediator mediator = scope.Mediator;

        await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.SendAsync<string>(null!));
    }

    [Fact]
    public async Task SendAsync_VoidWithNullRequest_ThrowsArgumentNullException()
    {
        using MediatorErrorTestScope scope = CreateEmptyMediator();
        IMediator mediator = scope.Mediator;

        await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.SendAsync(null!));
    }

    [Fact]
    public async Task SendAsync_WithMultipleHandlersForSameRequest_ThrowsInvalidOperationException()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddTransient<IMediator, Robotico.Mediator.Mediator>();
        services.AddTransient<IRequestHandler<UnhandledQueryForErrorTests, string>, MediatorErrorTestsDuplicateHandlerOne>();
        services.AddTransient<IRequestHandler<UnhandledQueryForErrorTests, string>, MediatorErrorTestsDuplicateHandlerTwo>();
        using ServiceProvider provider = services.BuildServiceProvider();
        IMediator mediator = provider.GetRequiredService<IMediator>();
        UnhandledQueryForErrorTests query = new("test");

        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.SendAsync(query));
    }
}
