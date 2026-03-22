using Robotico.Mediator;

namespace Robotico.Mediator.Tests.DuplicateScanHandlers;

/// <summary>
/// First of two handlers for <see cref="DuplicateScanRequest"/> (duplicate registration scenario).
/// </summary>
public sealed class DuplicateScanHandlerOne : IRequestHandler<DuplicateScanRequest, string>
{
    /// <inheritdoc />
    public Task<string> HandleAsync(DuplicateScanRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult("one");
}
