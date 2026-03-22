using Robotico.Mediator;

namespace Robotico.Mediator.Tests.Generated;

/// <summary>
/// Request type for source-generated mediator contract tests.
/// </summary>
public record GenPingQuery(string Value) : IRequest<string>;
