using System.Diagnostics.CodeAnalysis;

namespace Robotico.Mediator;

/// <summary>
/// Represents a delegate that invokes the next behavior or the final handler in the pipeline.
/// </summary>
/// <typeparam name="TResponse">The type of response expected from the pipeline.</typeparam>
/// <returns>A task representing the asynchronous operation, containing the response.</returns>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Delegate suffix is correct for a delegate type; standard pipeline pattern.")]
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();
