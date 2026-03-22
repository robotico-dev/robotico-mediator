using Robotico.Mediator;
using VoidResult = Robotico.Result.Result;

namespace Robotico.Mediator.Tests.Generated;

/// <summary>
/// Handler for <see cref="GenVoidCommand"/>.
/// </summary>
public sealed class GenVoidCommandHandler : IRequestHandler<GenVoidCommand>
{
    /// <inheritdoc />
    public Task<VoidResult> HandleAsync(GenVoidCommand request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(VoidResult.Success());
    }
}
