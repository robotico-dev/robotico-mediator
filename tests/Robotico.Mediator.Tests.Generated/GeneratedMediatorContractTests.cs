using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Robotico.Mediator;
using Robotico.Mediator.Generated;
using FluentAssertions;
using VoidResult = Robotico.Result.Result;
using Xunit;

namespace Robotico.Mediator.Tests.Generated;

/// <summary>
/// Contract tests for the source-generated mediator (GeneratedMediator).
/// Ensures the generated path behaves like the reflection-based Mediator for the same scenarios.
/// </summary>
public sealed class GeneratedMediatorContractTests
{
    private static GeneratedMediatorContractTestScope CreateGeneratedMediator()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddTransient<IMediator, GeneratedMediator>();
        services.AddTransient<IRequestHandler<GenPingQuery, string>, GenPingQueryHandler>();
        services.AddTransient<IRequestHandler<GenVoidCommand>, GenVoidCommandHandler>();
        ServiceProvider provider = services.BuildServiceProvider();
        return new GeneratedMediatorContractTestScope(provider, provider.GetRequiredService<IMediator>());
    }

    [Fact]
    public async Task SendAsync_WithTypedRequest_ReturnsHandlerResponse()
    {
        using GeneratedMediatorContractTestScope scope = CreateGeneratedMediator();
        IMediator mediator = scope.Mediator;
        GenPingQuery query = new("hello");

        string result = await mediator.SendAsync(query);

        result.Should().Be("HELLO");
    }

    [Fact]
    public async Task SendAsync_WithVoidRequest_ReturnsSuccessResult()
    {
        using GeneratedMediatorContractTestScope scope = CreateGeneratedMediator();
        IMediator mediator = scope.Mediator;
        GenVoidCommand command = new("test");

        VoidResult result = await mediator.SendAsync(command);

        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_VoidOverload_EquivalentToTypedResult()
    {
        using GeneratedMediatorContractTestScope scope = CreateGeneratedMediator();
        IMediator mediator = scope.Mediator;
        GenVoidCommand command = new("id");

        VoidResult fromVoid = await mediator.SendAsync(command);
        VoidResult fromTyped = await mediator.SendAsync<VoidResult>((IRequest<VoidResult>)command);

        fromVoid.IsSuccess().Should().BeTrue();
        fromTyped.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        using GeneratedMediatorContractTestScope scope = CreateGeneratedMediator();
        IMediator mediator = scope.Mediator;

        await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.SendAsync<string>(null!));
    }

    [Fact]
    public async Task SendAsync_VoidWithNullRequest_ThrowsArgumentNullException()
    {
        using GeneratedMediatorContractTestScope scope = CreateGeneratedMediator();
        IMediator mediator = scope.Mediator;

        await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.SendAsync(null!));
    }

    [Fact]
    public async Task SendAsync_WithNoRegisteredHandler_ThrowsMediatorNoHandlerException()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddTransient<IMediator, GeneratedMediator>();
        using ServiceProvider provider = services.BuildServiceProvider();
        IMediator mediator = provider.GetRequiredService<IMediator>();
        GenPingQuery query = new("unhandled");

        await Assert.ThrowsAsync<MediatorNoHandlerException>(() => mediator.SendAsync(query));
    }
}
