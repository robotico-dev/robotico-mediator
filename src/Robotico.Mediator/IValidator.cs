namespace Robotico.Mediator;

/// <summary>
/// Defines a validator for a request. Use with <see cref="ValidationPipelineBehavior"/> to run
/// validation before the handler; invalid requests short-circuit with <see cref="Robotico.Result.Result.ValidationError"/>.
/// </summary>
/// <typeparam name="TRequest">The type of request to validate.</typeparam>
public interface IValidator<in TRequest>
    where TRequest : IRequest
{
    /// <summary>
    /// Validates the request synchronously.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <returns>Success if valid; <see cref="Robotico.Result.Result.ValidationError"/> if invalid.</returns>
    Robotico.Result.Result Validate(TRequest request);
}
