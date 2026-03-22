using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Robotico.Mediator;
using Robotico.Mediator.Tests.DuplicateScanHandlers;
using Robotico.Mediator.Tests.MediatorInvariantPipelineTypes;
using Robotico.Mediator.Tests.ScanHandlers;
using VoidResult = Robotico.Result.Result;
using Xunit;

namespace Robotico.Mediator.Tests;

/// <summary>
/// Invariant tests: pipeline order (first registered = outermost), validation short-circuit (handler not invoked when validator fails),
/// duplicate handler at registration (AddMediator throws when assembly contains two handlers for same request).
/// </summary>
[Collection("MediatorScanHandlers")]
public sealed class MediatorInvariantTests
{
    [Fact]
    public async Task Pipeline_FirstRegisteredBehavior_IsOutermost()
    {
        List<string> order = [];
        ServiceCollection services = new();
        services.AddLogging();
        services.AddTransient<IRequestHandler<PipelineInvariantOrderQuery, int>, PipelineInvariantOrderQueryHandler>();
        services.AddTransient<IPipelineBehavior<IRequest<int>, int>>(
            _ => new PipelineInvariantOuterBehavior(order));
        services.AddTransient<IPipelineBehavior<IRequest<int>, int>>(
            _ => new PipelineInvariantInnerBehavior(order));
        services.AddTransient<IMediator, Robotico.Mediator.Mediator>();
        using ServiceProvider provider = services.BuildServiceProvider();
        IMediator mediator = provider.GetRequiredService<IMediator>();

        int result = await mediator.SendAsync(new PipelineInvariantOrderQuery(1));

        result.Should().Be(1);
        order.Should().ContainInOrder("Outer:Before", "Inner:Before", "Inner:After", "Outer:After");
    }

    [Fact]
    public async Task Validation_WhenInvalid_HandlerNotInvoked()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddMediator(typeof(ValidatedCommand).Assembly);
        using ServiceProvider provider = services.BuildServiceProvider();
        IMediator mediator = provider.GetRequiredService<IMediator>();
        ValidatedCommandHandler.Invoked = false;

        VoidResult result = await mediator.SendAsync(new ValidatedCommand("", -1));

        result.IsSuccess().Should().BeFalse();
        ValidatedCommandHandler.Invoked.Should().BeFalse("validation should short-circuit and not invoke handler");
    }

    [Fact]
    public void AddMediator_WhenAssemblyHasDuplicateHandlers_Throws()
    {
        ServiceCollection services = new();
        services.AddLogging();

        Action act = () => services.AddMediator(typeof(DuplicateScanRequest).Assembly);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Multiple handlers*DuplicateScanRequest*");
    }
}
