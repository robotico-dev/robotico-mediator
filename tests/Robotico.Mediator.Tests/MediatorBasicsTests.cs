using Microsoft.Extensions.DependencyInjection;
using Robotico.Mediator;
using VoidResult = Robotico.Result.Result;

namespace Robotico.Mediator.Tests;

/// <summary>
/// Tests for basic mediator functionality: sending requests and resolving handlers.
/// </summary>
public class MediatorBasicsTests
{
    #region Test Types

    private record TestQuery(string Value) : IRequest<string>;

    private sealed class TestQueryHandler : IRequestHandler<TestQuery, string>
    {
        public Task<string> HandleAsync(TestQuery request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(request.Value.ToUpperInvariant());
        }
    }

    private record TestVoidCommand(string Value) : IRequest;

    private sealed class TestVoidCommandHandler : IRequestHandler<TestVoidCommand>
    {
        public Task<VoidResult> HandleAsync(TestVoidCommand request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(VoidResult.Success());
        }
    }

    #endregion

    private static IMediator CreateMediator()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddTransient<IMediator, Robotico.Mediator.Mediator>();
        services.AddTransient<IRequestHandler<TestQuery, string>, TestQueryHandler>();
        services.AddTransient<IRequestHandler<TestVoidCommand>, TestVoidCommandHandler>();
        ServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task SendAsync_WithTypedRequest_ReturnsHandlerResponse()
    {
        IMediator mediator = CreateMediator();
        TestQuery query = new("hello");

        string result = await mediator.SendAsync(query);

        result.Should().Be("HELLO");
    }

    [Fact]
    public async Task SendAsync_WithVoidRequest_ReturnsSuccessResult()
    {
        IMediator mediator = CreateMediator();
        TestVoidCommand command = new("test");

        VoidResult result = await mediator.SendAsync(command);

        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_ResolvesHandlerFromServiceProvider()
    {
        IMediator mediator = CreateMediator();
        TestQuery query = new("world");

        string result = await mediator.SendAsync(query);

        result.Should().Be("WORLD");
    }
}
