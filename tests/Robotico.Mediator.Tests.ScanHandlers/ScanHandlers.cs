using Robotico.Mediator;
using VoidResult = Robotico.Result.Result;

namespace Robotico.Mediator.Tests.ScanHandlers;

/// <summary>
/// Request and handler types used only for AddMediator assembly-scan tests.
/// Isolated in a separate assembly so the main test assembly can contain duplicate handlers (MediatorErrorTests) without breaking scan.
/// </summary>
public record ScanQuery(string Value) : IRequest<string>;

public sealed class ScanQueryHandler : IRequestHandler<ScanQuery, string>
{
    public Task<string> HandleAsync(ScanQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(request.Value);
    }
}

public record ScanCommand : IRequest;

public sealed class ScanCommandHandler : IRequestHandler<ScanCommand>
{
    public Task<VoidResult> HandleAsync(ScanCommand request, CancellationToken cancellationToken = default) =>
        Task.FromResult(VoidResult.Success());
}

public record ValidatedCommand(string Name, int Age) : IRequest;

public sealed class ValidatedCommandHandler : IRequestHandler<ValidatedCommand>
{
    public Task<VoidResult> HandleAsync(ValidatedCommand request, CancellationToken cancellationToken = default) =>
        Task.FromResult(VoidResult.Success());
}

public sealed class ValidatedCommandValidator : IValidator<ValidatedCommand>
{
    public VoidResult Validate(ValidatedCommand request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Dictionary<string, string[]> errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Name))
            errors["Name"] = ["Name is required"];
        if (request.Age < 0 || request.Age > 150)
            errors["Age"] = ["Age must be between 0 and 150"];
        return errors.Count == 0 ? VoidResult.Success() : VoidResult.ValidationError(errors);
    }
}
