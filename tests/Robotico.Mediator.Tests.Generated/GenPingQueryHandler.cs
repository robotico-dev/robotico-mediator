using Robotico.Mediator;

namespace Robotico.Mediator.Tests.Generated;

/// <summary>
/// Handler for <see cref="GenPingQuery"/> (seen by mediator source generator).
/// </summary>
public sealed class GenPingQueryHandler : IRequestHandler<GenPingQuery, string>
{
    /// <inheritdoc />
    public Task<string> HandleAsync(GenPingQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(request.Value.ToUpperInvariant());
    }
}
