using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Robotico.Mediator;

namespace Robotico.Mediator.Tests;

/// <summary>
/// Tests that when a handler or pipeline behavior throws, the mediator logs at Warning and rethrows the same exception (no swallowing).
/// </summary>
public class MediatorExceptionPropagationTests
{
    #region Test types

    private record ThrowingQuery(string Id) : IRequest<string>;

    private sealed class ThrowingHandler : IRequestHandler<ThrowingQuery, string>
    {
        public Task<string> HandleAsync(ThrowingQuery request, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Handler failed: " + request.Id);
        }
    }

    #endregion

    private static (IMediator mediator, List<LogEntry> logEntries) CreateMediatorWithLogCapture()
    {
        List<LogEntry> logEntries = new();
        ServiceCollection services = new();
        services.AddLogging(builder =>
        {
            builder.AddProvider(new ListLoggerProvider(logEntries));
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        services.AddTransient<IMediator, Robotico.Mediator.Mediator>();
        services.AddTransient<IRequestHandler<ThrowingQuery, string>, ThrowingHandler>();
        IMediator mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
        return (mediator, logEntries);
    }

    [Fact]
    public async Task SendAsync_WhenHandlerThrows_PropagatesSameException()
    {
        (IMediator mediator, List<LogEntry> logEntries) = CreateMediatorWithLogCapture();
        ThrowingQuery request = new("x");

        InvalidOperationException? thrown = await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.SendAsync(request));

        thrown.Message.Should().Be("Handler failed: x");
    }

    [Fact]
    public async Task SendAsync_WhenHandlerThrows_LogsWarningThenRethrows()
    {
        (IMediator mediator, List<LogEntry> logEntries) = CreateMediatorWithLogCapture();
        ThrowingQuery request = new("y");

        InvalidOperationException? thrown = await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.SendAsync(request));

        logEntries.Should().ContainSingle(e =>
            e.LogLevel == LogLevel.Warning &&
            e.Exception == thrown &&
            e.Message.Contains("failed", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class LogEntry
    {
        public LogLevel LogLevel { get; set; }
        public Exception? Exception { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    private sealed class ListLogger : ILogger
    {
        private readonly List<LogEntry> _entries;
        private readonly string _category;

        public ListLogger(List<LogEntry> entries, string category)
        {
            _entries = entries;
            _category = category;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _entries.Add(new LogEntry
            {
                LogLevel = logLevel,
                Exception = exception,
                Message = formatter(state, exception),
            });
        }
    }

    private sealed class ListLoggerProvider : ILoggerProvider
    {
        private readonly List<LogEntry> _entries;

        public ListLoggerProvider(List<LogEntry> entries)
        {
            _entries = entries;
        }

        public ILogger CreateLogger(string categoryName) => new ListLogger(_entries, categoryName);

        public void Dispose() { }
    }
}
