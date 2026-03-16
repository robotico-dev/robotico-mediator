using System.Collections.Concurrent;
using System.Reflection;

namespace Robotico.Mediator;

/// <summary>
/// Pipeline behavior that runs <see cref="IValidator{TRequest}"/> before the handler when a validator is registered for the request type.
/// If validation fails, the pipeline short-circuits and returns the validation error result without invoking the handler.
/// Register this behavior once for void/command requests and register validators per request type.
/// Validator interface type and <see cref="MethodInfo"/> are cached per request type to avoid reflection on the hot path.
/// </summary>
public sealed class ValidationPipelineBehavior(IServiceProvider serviceProvider) : IPipelineBehavior<IRequest<Robotico.Result.Result>, Robotico.Result.Result>
{
    private static readonly ConcurrentDictionary<Type, (Type ValidatorType, MethodInfo? ValidateMethod)> ValidatorInfoCache = new();

    /// <inheritdoc />
    public Task<Robotico.Result.Result> HandleAsync(IRequest<Robotico.Result.Result> request, RequestHandlerDelegate<Robotico.Result.Result> next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        Type requestType = request.GetType();
        (Type validatorType, MethodInfo? validateMethod) = GetOrAddValidatorInfo(requestType);
        object? validator = serviceProvider.GetService(validatorType);
        if (validator is null)
        {
            return next();
        }

        if (validateMethod is null)
        {
            return next();
        }

        Robotico.Result.Result validationResult = (Robotico.Result.Result)validateMethod.Invoke(validator, [request])!;
        return validationResult.IsSuccess() ? next() : Task.FromResult(validationResult);
    }

    private static (Type ValidatorType, MethodInfo? ValidateMethod) GetOrAddValidatorInfo(Type requestType)
    {
        if (ValidatorInfoCache.TryGetValue(requestType, out (Type ValidatorType, MethodInfo? ValidateMethod) cached))
        {
            return cached;
        }

        Type validatorType = typeof(IValidator<>).MakeGenericType(requestType);
        MethodInfo? validateMethod = validatorType.GetMethod(nameof(IValidator<IRequest>.Validate), [requestType]);
        cached = (validatorType, validateMethod);
        ValidatorInfoCache.TryAdd(requestType, cached);
        return cached;
    }
}
