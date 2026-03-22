using System.Diagnostics.CodeAnalysis;

namespace Robotico.Mediator;

/// <summary>
/// Thrown when <see cref="IMediator.SendAsync{TResponse}(IRequest{TResponse}, CancellationToken)"/> or
/// <see cref="IMediator.SendAsync(IRequest, CancellationToken)"/> is called but no handler is registered for the request type.
/// </summary>
[SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Single factory constructor with requestTypeName is the intended API; aligns with Robotico.Result exception patterns.")]
public sealed class MediatorNoHandlerException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance with a stable message derived from <paramref name="requestTypeName"/>.
    /// </summary>
    /// <param name="requestTypeName">The CLR name of the request type (e.g. from <c>Type.Name</c>).</param>
    public MediatorNoHandlerException(string requestTypeName)
        : base($"No handler registered for request type {requestTypeName}.")
    {
        ArgumentNullException.ThrowIfNull(requestTypeName);
        RequestTypeName = requestTypeName;
    }

    /// <summary>
    /// Gets the CLR type name of the request that had no handler.
    /// </summary>
    public string RequestTypeName { get; }
}
