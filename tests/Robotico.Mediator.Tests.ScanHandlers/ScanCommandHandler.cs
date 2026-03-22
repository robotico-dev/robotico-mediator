using Robotico.Mediator;
using VoidResult = Robotico.Result.Result;

namespace Robotico.Mediator.Tests.ScanHandlers;

/// <summary>
/// Handler for <see cref="ScanCommand"/>.
/// </summary>
public sealed class ScanCommandHandler : IRequestHandler<ScanCommand>
{
    /// <inheritdoc />
    public Task<VoidResult> HandleAsync(ScanCommand request, CancellationToken cancellationToken = default) =>
        Task.FromResult(VoidResult.Success());
}
