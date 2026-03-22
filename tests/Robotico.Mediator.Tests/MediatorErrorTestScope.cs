using Microsoft.Extensions.DependencyInjection;
using Robotico.Mediator;

namespace Robotico.Mediator.Tests;

internal sealed class MediatorErrorTestScope : IDisposable
{
    private readonly ServiceProvider _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediatorErrorTestScope"/> class.
    /// </summary>
    public MediatorErrorTestScope(ServiceProvider provider, IMediator mediator)
    {
        _provider = provider;
        Mediator = mediator;
    }

    /// <summary>
    /// Gets the mediator under test.
    /// </summary>
    public IMediator Mediator { get; }

    /// <inheritdoc />
    public void Dispose() => _provider.Dispose();
}
