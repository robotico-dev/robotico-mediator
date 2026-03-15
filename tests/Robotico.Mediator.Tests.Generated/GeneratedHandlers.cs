using Robotico.Mediator;
using VoidResult = Robotico.Result.Result;

namespace Robotico.Mediator.Tests.Generated;

/// <summary>
/// Request and handler types used only for source-generator contract tests.
/// Defined in this assembly so the generator sees them and emits GeneratedMediator dispatch for them.
/// </summary>
public record GenPingQuery(string Value) : IRequest<string>;

public sealed class GenPingQueryHandler : IRequestHandler<GenPingQuery, string>
{
    public Task<string> HandleAsync(GenPingQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(request.Value.ToUpperInvariant());
    }
}

public record GenVoidCommand(string Id) : IRequest;

public sealed class GenVoidCommandHandler : IRequestHandler<GenVoidCommand>
{
    public Task<VoidResult> HandleAsync(GenVoidCommand request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VoidResult.Success());
    }
}
