using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Robotico.Mediator;

/// <summary>
/// Extension methods for registering the mediator and its handlers in the dependency injection container.
/// </summary>
public static class MediatorServiceCollectionExtensions
{
    /// <summary>
    /// Adds the mediator and scans the specified assemblies for request handlers and pipeline behaviors.
    /// Handlers are registered as transient services. The mediator is registered with the specified lifetime.
    /// </summary>
    /// <param name="services">The service collection to add the mediator to.</param>
    /// <param name="assemblies">The assemblies to scan for handlers and pipeline behaviors.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>Ensure logging is registered (e.g. <c>AddLogging()</c>) so the mediator can log request lifecycle events.</remarks>
    /// <exception cref="ArgumentException">Thrown when no assemblies are provided.</exception>
    /// <exception cref="InvalidOperationException">Thrown when more than one handler is found for the same request type.</exception>
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
        => AddMediator(services, ServiceLifetime.Transient, assemblies);

    /// <summary>
    /// Adds the mediator and scans the specified assemblies for request handlers and pipeline behaviors.
    /// Handlers are registered as transient services. The mediator is registered with the specified lifetime.
    /// </summary>
    /// <param name="services">The service collection to add the mediator to.</param>
    /// <param name="mediatorLifetime">The lifetime of the <see cref="IMediator"/> service (e.g. <see cref="ServiceLifetime.Scoped"/> for one per request in web apps).</param>
    /// <param name="assemblies">The assemblies to scan for handlers and pipeline behaviors.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>Ensure logging is registered (e.g. <c>AddLogging()</c>) so the mediator can log request lifecycle events.</remarks>
    /// <exception cref="ArgumentException">Thrown when no assemblies are provided.</exception>
    /// <exception cref="InvalidOperationException">Thrown when more than one handler is found for the same request type.</exception>
    public static IServiceCollection AddMediator(this IServiceCollection services, ServiceLifetime mediatorLifetime, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);
        if (assemblies.Length == 0)
        {
            throw new ArgumentException("At least one assembly must be provided for scanning.", nameof(assemblies));
        }

        services.Add(new ServiceDescriptor(typeof(IMediator), typeof(Mediator), mediatorLifetime));

        RegisterHandlers(services, assemblies);
        RegisterPipelineBehaviors(services, assemblies);
        RegisterValidators(services, assemblies);
        services.AddTransient<IPipelineBehavior<IRequest<Robotico.Result.Result>, Robotico.Result.Result>, ValidationPipelineBehavior>();

        return services;
    }

    /// <summary>
    /// Adds the mediator and scans the specified assemblies for request handlers and pipeline behaviors.
    /// The mediator is registered as scoped (one instance per scope, e.g. per HTTP request).
    /// </summary>
    /// <param name="services">The service collection to add the mediator to.</param>
    /// <param name="assemblies">The assemblies to scan for handlers and pipeline behaviors.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when no assemblies are provided.</exception>
    /// <exception cref="InvalidOperationException">Thrown when more than one handler is found for the same request type.</exception>
    public static IServiceCollection AddMediatorScoped(this IServiceCollection services, params Assembly[] assemblies)
        => AddMediator(services, ServiceLifetime.Scoped, assemblies);

    private static void RegisterHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        Type[] handlerInterfaceTypes =
        [
            typeof(IRequestHandler<,>),
            typeof(IRequestHandler<>)
        ];

        Dictionary<Type, Type> handlerInterfaceToImplementation = new();

        foreach (Assembly assembly in assemblies)
        {
            IEnumerable<Type> types = assembly.GetTypes()
                .Where(type => type is { IsAbstract: false, IsInterface: false });

            foreach (Type type in types)
            {
                foreach (Type interfaceType in type.GetInterfaces())
                {
                    if (!interfaceType.IsGenericType)
                    {
                        continue;
                    }

                    Type genericDefinition = interfaceType.GetGenericTypeDefinition();

                    if (handlerInterfaceTypes.Contains(genericDefinition))
                    {
                        if (handlerInterfaceToImplementation.TryGetValue(interfaceType, out Type? existing) && existing != type)
                        {
                            throw new InvalidOperationException(
                                $"Multiple handlers for the same request type: both {existing.FullName} and {type.FullName} implement {interfaceType}. Register only one handler per request type.");
                        }

                        handlerInterfaceToImplementation[interfaceType] = type;
                        services.AddTransient(interfaceType, type);
                    }
                }
            }
        }
    }

    private static void RegisterValidators(IServiceCollection services, Assembly[] assemblies)
    {
        Type validatorType = typeof(IValidator<>);

        foreach (Assembly assembly in assemblies)
        {
            IEnumerable<Type> types = assembly.GetTypes()
                .Where(type => type is { IsAbstract: false, IsInterface: false });

            foreach (Type type in types)
            {
                foreach (Type interfaceType in type.GetInterfaces())
                {
                    if (!interfaceType.IsGenericType)
                    {
                        continue;
                    }

                    if (interfaceType.GetGenericTypeDefinition() == validatorType)
                    {
                        services.AddTransient(interfaceType, type);
                        break;
                    }
                }
            }
        }
    }

    private static void RegisterPipelineBehaviors(IServiceCollection services, Assembly[] assemblies)
    {
        Type pipelineBehaviorType = typeof(IPipelineBehavior<,>);

        foreach (Assembly assembly in assemblies)
        {
            IEnumerable<Type> types = assembly.GetTypes()
                .Where(type => type is { IsAbstract: false, IsInterface: false });

            foreach (Type type in types)
            {
                foreach (Type interfaceType in type.GetInterfaces())
                {
                    if (!interfaceType.IsGenericType)
                    {
                        continue;
                    }

                    Type genericDefinition = interfaceType.GetGenericTypeDefinition();

                    if (genericDefinition == pipelineBehaviorType)
                    {
                        services.AddTransient(interfaceType, type);
                    }
                }
            }
        }
    }
}
