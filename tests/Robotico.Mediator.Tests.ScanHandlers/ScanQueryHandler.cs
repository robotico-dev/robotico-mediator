using Robotico.Mediator;

namespace Robotico.Mediator.Tests.ScanHandlers;

/// <summary>
/// Handler for <see cref="ScanQuery"/> in scan tests.
/// </summary>
public sealed class ScanQueryHandler : IRequestHandler<ScanQuery, string>
{
    /// <inheritdoc />
    public Task<string> HandleAsync(ScanQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(request.Value);
    }
}
