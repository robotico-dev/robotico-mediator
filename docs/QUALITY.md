# Robotico.Mediator — Quality bar (senior/principal 10/10)

This document defines the target quality level for the mediator library and how it aligns with **Robotico.Result** (robotico-results-csharp). Both repos target the same senior/principal bar (10/10).

## Seniority rating

**Current assessment: 10/10** (senior/principal). The implementation is production-ready, SOLID, well-tested (unit, contract, edge-case, source-generator contract tests), and documented. It matches the same senior/principal bar as Robotico.Result.

## What already meets the bar

- **Architecture**: Mediator/CQRS, single handler per request, pipeline behaviors, assembly-scan registration.
- **SOLID**: Interfaces for mediator, requests, handlers, behaviors; dependency on `IServiceProvider` and `ILogger<Mediator>`.
- **Error handling**: Null checks (`ArgumentNullException.ThrowIfNull`), no handler / multiple handlers throw with clear messages; handler exceptions unwrapped and rethrown (no swallowing).
- **Testing**: Unit and contract tests (xUnit, FluentAssertions); real DI and mediator; pipeline order, validation, cancellation, duplicate-handler (runtime and scan-time); source-generator contract tests.
- **Documentation**: XML docs on public API (including <c>AddMediator</c> remark to register logging); README; `docs/index.adoc`, `docs/architecture.adoc`, `docs/design.adoc`, `docs/trim-aot.adoc`; source generator trade-offs documented in README and design.
- **Tooling**: TreatWarningsAsErrors, AnalysisLevel latest-all, EnforceCodeStyleInBuild (Directory.Build.props); SourceLink.
- **Production readiness**: Nullable, structured logging with event IDs (`MediatorEventIds`). When publishing as a standalone NuGet package, use a PackageReference to Robotico.Result instead of a ProjectReference.

## How to reach / maintain 10/10

1. **SOLID & abstractions** — Keep interfaces for IMediator, requests, handlers, behaviors; resolve via DI only.
2. **Error handling** — Validate arguments at boundaries; no handler / multiple handlers throw; do not swallow handler exceptions.
3. **Tests** — Unit + contract tests (reflection and generated mediator); edge cases (duplicate handlers, no validator). Optional: theory/parameterized tests for pipeline order or request types.
4. **Coverage** — Coverlet with documented command; optional CI gate (e.g. 80% line). See README.
5. **Documentation** — Full XML docs on public API; README; docs/ (index, architecture, design, trim-aot).
6. **Analyzers** — TreatWarningsAsErrors, latest analyzers, EnforceCodeStyleInBuild. Document any intentional suppressions.
7. **Observability** — Structured logging with event IDs; optional: sample or doc for OpenTelemetry `Activity` in a pipeline behavior.

Optional enhancements (do not affect 10/10): integration test (Minimal API/WebApplicationFactory); observability sample (IPipelineBehavior with Activity).

## Shared bar with Robotico.Result

Both libraries aim for the same senior/principal bar (10/10). Criteria:

| Criterion            | Robotico.Result           | Robotico.Mediator              |
|----------------------|---------------------------|---------------------------------|
| SOLID & abstractions | Yes                       | Yes                             |
| Error handling       | Result + boundary         | Exceptions + Result             |
| Tests                | Unit + theory + CsCheck   | Unit + contract + edge cases     |
| Coverage             | Documented + optional gate | Documented + optional gate     |
| XML docs             | Full public API           | Full public API                 |
| Design docs          | index, architecture, design, trim-aot | index, architecture, design, trim-aot |
| Analyzers            | Latest, warnings as errors | Latest, warnings as errors      |
| Observability        | N/A (library)             | Logging + event IDs             |

## Optional future improvements

- **Integration test**: Minimal API or WebApplicationFactory that sends a request through the full stack.
- **Optional tracing**: Sample or documented `IPipelineBehavior` that creates an `Activity` per request for OpenTelemetry.
