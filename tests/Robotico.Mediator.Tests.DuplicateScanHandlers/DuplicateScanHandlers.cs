using Robotico.Mediator;

namespace Robotico.Mediator.Tests.DuplicateScanHandlers;

/// <summary>
/// Assembly that intentionally contains two handlers for the same request type.
/// Used only to test that AddMediator throws InvalidOperationException when scanning such an assembly.
/// </summary>
public record DuplicateScanRequest(int Id) : IRequest<string>;

public sealed class DuplicateScanHandlerOne : IRequestHandler<DuplicateScanRequest, string>
{
    public Task<string> HandleAsync(DuplicateScanRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult("one");
}

public sealed class DuplicateScanHandlerTwo : IRequestHandler<DuplicateScanRequest, string>
{
    public Task<string> HandleAsync(DuplicateScanRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult("two");
}
