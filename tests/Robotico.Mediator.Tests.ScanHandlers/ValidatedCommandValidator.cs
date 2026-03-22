using Robotico.Mediator;
using VoidResult = Robotico.Result.Result;

namespace Robotico.Mediator.Tests.ScanHandlers;

/// <summary>
/// Validator for <see cref="ValidatedCommand"/> used in mediator validation tests.
/// </summary>
public sealed class ValidatedCommandValidator : IValidator<ValidatedCommand>
{
    /// <inheritdoc />
    public VoidResult Validate(ValidatedCommand request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Dictionary<string, string[]> errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["Name"] = new[] { "Name is required" };
        }

        if (request.Age < 0 || request.Age > 150)
        {
            errors["Age"] = new[] { "Age must be between 0 and 150" };
        }

        return errors.Count == 0 ? VoidResult.Success() : VoidResult.ValidationError(errors);
    }
}
