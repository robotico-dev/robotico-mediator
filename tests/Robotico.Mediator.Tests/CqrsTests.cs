using Microsoft.Extensions.DependencyInjection;
using Robotico.Mediator;
using VoidResult = Robotico.Result.Result;

namespace Robotico.Mediator.Tests;

/// <summary>
/// Tests for CQRS type safety: commands cannot be handled by query handlers and vice versa.
/// </summary>
public class CqrsTests
{
    #region Test Types

    private record CreateItemCommand(string Name) : ICommand<int>;

    private sealed class CreateItemCommandHandler : ICommandHandler<CreateItemCommand, int>
    {
        public Task<int> HandleAsync(CreateItemCommand request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(42);
        }
    }

    private record DeleteItemCommand(int Id) : ICommand;

    private sealed class DeleteItemCommandHandler : ICommandHandler<DeleteItemCommand>
    {
        public Task<VoidResult> HandleAsync(DeleteItemCommand request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(VoidResult.Success());
        }
    }

    private record GetItemQuery(int Id) : IQuery<string>;

    private sealed class GetItemQueryHandler : IQueryHandler<GetItemQuery, string>
    {
        public Task<string> HandleAsync(GetItemQuery request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"Item-{request.Id}");
        }
    }

    #endregion

    private sealed class MediatorScope : IDisposable
    {
        private readonly ServiceProvider _provider;
        public IMediator Mediator { get; }
        public MediatorScope(ServiceProvider provider, IMediator mediator) { _provider = provider; Mediator = mediator; }
        public void Dispose() => _provider.Dispose();
    }

    private static MediatorScope CreateMediator()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddTransient<IMediator, Robotico.Mediator.Mediator>();
        services.AddTransient<IRequestHandler<CreateItemCommand, int>, CreateItemCommandHandler>();
        services.AddTransient<IRequestHandler<DeleteItemCommand>, DeleteItemCommandHandler>();
        services.AddTransient<IRequestHandler<GetItemQuery, string>, GetItemQueryHandler>();
        ServiceProvider provider = services.BuildServiceProvider();
        return new MediatorScope(provider, provider.GetRequiredService<IMediator>());
    }

    [Fact]
    public async Task Command_WithResponse_IsHandledByCommandHandler()
    {
        using MediatorScope scope = CreateMediator();
        IMediator mediator = scope.Mediator;
        CreateItemCommand command = new("TestItem");

        int result = await mediator.SendAsync(command);

        result.Should().Be(42);
    }

    [Fact]
    public async Task Command_WithoutResponse_IsHandledByCommandHandler()
    {
        using MediatorScope scope = CreateMediator();
        IMediator mediator = scope.Mediator;
        DeleteItemCommand command = new(1);

        VoidResult result = await mediator.SendAsync(command);

        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task Query_IsHandledByQueryHandler()
    {
        using MediatorScope scope = CreateMediator();
        IMediator mediator = scope.Mediator;
        GetItemQuery query = new(5);

        string result = await mediator.SendAsync(query);

        result.Should().Be("Item-5");
    }

    [Fact]
    public void Command_ImplementsIRequest()
    {
        typeof(ICommand).Should().BeAssignableTo<IRequest>();
        typeof(ICommand<int>).Should().BeAssignableTo<IRequest<int>>();
    }

    [Fact]
    public void Query_ImplementsIRequest()
    {
        typeof(IQuery<string>).Should().BeAssignableTo<IRequest<string>>();
    }

    [Fact]
    public void CommandHandler_ImplementsIRequestHandler()
    {
        typeof(ICommandHandler<DeleteItemCommand>).Should().BeAssignableTo<IRequestHandler<DeleteItemCommand>>();
        typeof(ICommandHandler<CreateItemCommand, int>).Should().BeAssignableTo<IRequestHandler<CreateItemCommand, int>>();
    }

    [Fact]
    public void QueryHandler_ImplementsIRequestHandler()
    {
        typeof(IQueryHandler<GetItemQuery, string>).Should().BeAssignableTo<IRequestHandler<GetItemQuery, string>>();
    }
}
