using Robotico.Mediator;
using Robotico.Mediator.Tests.MediatorErrorTestRecords;

namespace Robotico.Mediator.Tests.MediatorErrorTestHandlers;

internal sealed class MediatorErrorTestsDuplicateHandlerOne : IRequestHandler<UnhandledQueryForErrorTests, string>
{
    /// <inheritdoc />
    public Task<string> HandleAsync(UnhandledQueryForErrorTests request, CancellationToken cancellationToken = default) =>
        Task.FromResult("one");
}
