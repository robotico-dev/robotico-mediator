using Robotico.Mediator;

namespace Robotico.Mediator.Tests.DuplicateScanHandlers;

/// <summary>
/// Second of two handlers for <see cref="DuplicateScanRequest"/> (duplicate registration scenario).
/// </summary>
public sealed class DuplicateScanHandlerTwo : IRequestHandler<DuplicateScanRequest, string>
{
    /// <inheritdoc />
    public Task<string> HandleAsync(DuplicateScanRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult("two");
}
