using Microsoft.Extensions.DependencyInjection;
using Robotico.Mediator;
using VoidResult = Robotico.Result.Result;

namespace Robotico.Mediator.Tests;

/// <summary>
/// Tests for AddMediator assembly scan registration.
/// </summary>
public class MediatorRegistrationTests
{
    private static System.Reflection.Assembly ScanAssembly => typeof(Robotico.Mediator.Tests.ScanHandlers.ScanQuery).Assembly;

    [Fact]
    public async Task AddMediator_ScansAssembly_ResolvesHandlers()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddSingleton(new List<string>());
        services.AddMediator(ScanAssembly);
        ServiceProvider provider = services.BuildServiceProvider();

        IMediator mediator = provider.GetRequiredService<IMediator>();

        VoidResult voidResult = await mediator.SendAsync(new Robotico.Mediator.Tests.ScanHandlers.ScanCommand());
        voidResult.IsSuccess().Should().BeTrue();

        string queryResult = await mediator.SendAsync(new Robotico.Mediator.Tests.ScanHandlers.ScanQuery("scanned"));
        queryResult.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AddMediator_WithNullServices_ThrowsArgumentNullException()
    {
        Action act = () => MediatorServiceCollectionExtensions.AddMediator(null!, ScanAssembly);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddMediator_WithNoAssemblies_ThrowsArgumentException()
    {
        ServiceCollection services = new();

        Action act = () => services.AddMediator(Array.Empty<System.Reflection.Assembly>());

        act.Should().Throw<ArgumentException>()
            .WithParameterName("assemblies");
    }

    [Fact]
    public void AddMediatorScoped_RegistersMediatorAsScoped()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddMediatorScoped(ScanAssembly);
        ServiceProvider provider = services.BuildServiceProvider();

        using IServiceScope scope1 = provider.CreateScope();
        using IServiceScope scope2 = provider.CreateScope();
        IMediator m1 = scope1.ServiceProvider.GetRequiredService<IMediator>();
        IMediator m2 = scope2.ServiceProvider.GetRequiredService<IMediator>();

        m1.Should().NotBeSameAs(m2);
    }

    /// <summary>
    /// When an assembly contains two handlers for the same request type, AddMediator throws at scan time.
    /// </summary>
    [Fact]
    public void AddMediator_WithDuplicateHandlersInAssembly_ThrowsInvalidOperationException()
    {
        ServiceCollection services = new();
        System.Reflection.Assembly duplicateAssembly = typeof(Robotico.Mediator.Tests.DuplicateScanHandlers.DuplicateScanRequest).Assembly;

        Action act = () => services.AddMediator(duplicateAssembly);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Multiple handlers*")
            .WithMessage("*DuplicateScanRequest*");
    }
}
