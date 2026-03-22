using Microsoft.Extensions.DependencyInjection;
using Robotico.Mediator;
using Robotico.Mediator.Tests.ScanHandlers;
using VoidResult = Robotico.Result.Result;

namespace Robotico.Mediator.Tests;

/// <summary>
/// Tests for ValidationPipelineBehavior and IValidator integration.
/// Uses the ScanHandlers assembly so assembly scan does not pick up duplicate handlers from the main test assembly.
/// </summary>
[Collection("MediatorScanHandlers")]
public sealed class ValidationPipelineTests
{
    [Fact]
    public async Task ValidationPipeline_WhenValid_InvokesHandler()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddMediator(typeof(ValidatedCommand).Assembly);
        using ServiceProvider provider = services.BuildServiceProvider();
        IMediator mediator = provider.GetRequiredService<IMediator>();

        VoidResult result = await mediator.SendAsync(new ValidatedCommand("Alice", 30));

        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
    public async Task ValidationPipeline_WhenInvalid_ShortCircuitsWithValidationError()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddMediator(typeof(ValidatedCommand).Assembly);
        using ServiceProvider provider = services.BuildServiceProvider();
        IMediator mediator = provider.GetRequiredService<IMediator>();

        VoidResult result = await mediator.SendAsync(new ValidatedCommand("", -1));

        result.IsSuccess().Should().BeFalse();
        result.IsError(out Robotico.Result.Errors.IError? error).Should().BeTrue();
        error.Should().BeOfType<Robotico.Result.Errors.ValidationError>();
    }

    /// <summary>
    /// When no IValidator is registered for a request type, ValidationPipelineBehavior calls next() and the handler runs.
    /// </summary>
    [Fact]
    public async Task ValidationPipeline_WhenNoValidatorRegistered_InvokesHandler()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddMediator(typeof(ValidatedCommand).Assembly);
        using ServiceProvider provider = services.BuildServiceProvider();
        IMediator mediator = provider.GetRequiredService<IMediator>();

        VoidResult result = await mediator.SendAsync(new ScanHandlers.ScanCommand());

        result.IsSuccess().Should().BeTrue();
    }
}
