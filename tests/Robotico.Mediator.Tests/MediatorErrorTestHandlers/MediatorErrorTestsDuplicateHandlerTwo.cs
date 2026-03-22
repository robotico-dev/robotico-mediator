using Robotico.Mediator;
using Robotico.Mediator.Tests.MediatorErrorTestRecords;

namespace Robotico.Mediator.Tests.MediatorErrorTestHandlers;

internal sealed class MediatorErrorTestsDuplicateHandlerTwo : IRequestHandler<UnhandledQueryForErrorTests, string>
{
    /// <inheritdoc />
    public Task<string> HandleAsync(UnhandledQueryForErrorTests request, CancellationToken cancellationToken = default) =>
        Task.FromResult("two");
}
