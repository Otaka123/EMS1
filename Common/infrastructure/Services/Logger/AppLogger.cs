using Common.Application.Contracts.interfaces;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Common.infrastructure.Services.Logger
{
    public class AppLogger<T> : IAppLogger<T> where T : class
    {
        private readonly ILogger<T> _logger;
        //private readonly IGlobalLogger _globalLogger;

        public AppLogger(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void LogTrace(string message, params object[] args)
            => _logger.LogTrace(message, args);

        public void LogDebug(string message, params object[] args)
            => _logger.LogDebug(message, args);

        public void LogInformation(string message, params object[] args)
            => _logger.LogInformation(message, args);

        public void LogWarning(string message, params object[] args)
            => _logger.LogWarning(message, args);

        public void LogError(string message, params object[] args)
            => _logger.LogError(message, args);
        public void LogError(Exception ex,string message, params object[] args)
          => _logger.LogError(ex,message, args);
        public void LogCritical(string message, params object[] args)
            => _logger.LogCritical(message, args);

        public void LogPerformance(string operationName, Action operation)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                operation();
                _logger.LogInformation("[Performance] {OperationName} completed in {ElapsedMs}ms",
                    operationName, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Performance] {OperationName} failed after {ElapsedMs}ms",
                    operationName, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task LogPerformanceAsync(string operationName, Func<Task> operation)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await operation();
                _logger.LogInformation("[Performance] {OperationName} completed in {ElapsedMs}ms",
                    operationName, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Performance] {OperationName} failed after {ElapsedMs}ms",
                    operationName, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public void LogRequestResponse(object request, object response, TimeSpan duration)
        {
            var logEntry = new
            {
                Request = request,
                Response = response,
                DurationMs = duration.TotalMilliseconds,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Request/Response: {@LogEntry}", logEntry);
        }

        public void LogValidationErrors(IEnumerable<ValidationFailure> errors)
        {
            foreach (var error in errors)
            {
                _logger.LogWarning("Validation Error - {Property}: {ErrorMessage}",
                    error.PropertyName, error.ErrorMessage);
            }
        }

        public void LogException(Exception ex, string? message = null, params object[] args)
        {
            var logMessage = message ?? "An exception occurred";
            _logger.LogError(ex, logMessage, args);
            //_globalLogger.LogSystemEvent("Exception", logMessage, new { Exception = ex });
        }

        public void LogSecurityEvent(string eventType, string message, object? additionalData = null)
        {
            _logger.LogWarning("[Security] {EventType}: {Message} - {@AdditionalData}",
                eventType, message, additionalData);
            //_globalLogger.LogSystemEvent("Security", message, additionalData);
        }

        public void LogPerformance(string operation, long milliseconds)
        {
            _logger.LogWarning($"[PERF] {operation} took {milliseconds}ms");


        }

        public void LogWarning(Exception ex,string message, params object[] args)
            => _logger.LogWarning(ex,message, args);
    }

 
}
