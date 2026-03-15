namespace Robotico.Mediator;

/// <summary>
/// Event IDs used for structured logging in <see cref="Mediator"/>.
/// Enables log aggregation and filtering by event in production.
/// </summary>
internal static class MediatorEventIds
{
    /// <summary>Request dispatch started.</summary>
    internal const int RequestStarted = 1;

    /// <summary>Request completed successfully.</summary>
    internal const int RequestCompleted = 2;

    /// <summary>Request failed with an exception.</summary>
    internal const int RequestFailed = 3;
}
