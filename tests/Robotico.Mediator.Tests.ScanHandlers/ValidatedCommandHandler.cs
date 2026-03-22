using Robotico.Mediator;
using VoidResult = Robotico.Result.Result;

namespace Robotico.Mediator.Tests.ScanHandlers;

/// <summary>
/// Handler for <see cref="ValidatedCommand"/>; <see cref="Invoked"/> supports short-circuit assertions.
/// </summary>
public sealed class ValidatedCommandHandler : IRequestHandler<ValidatedCommand>
{
    /// <summary>Set by tests to assert handler was or was not invoked (e.g. when validation short-circuits).</summary>
    public static bool Invoked { get; set; }

    /// <inheritdoc />
    public Task<VoidResult> HandleAsync(ValidatedCommand request, CancellationToken cancellationToken = default)
    {
        Invoked = true;
        return Task.FromResult(VoidResult.Success());
    }
}
