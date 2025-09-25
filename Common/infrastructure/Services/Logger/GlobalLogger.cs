using Common.Application.Contracts.interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.infrastructure.Services.Logger
{
    public class GlobalLogger : IGlobalLogger
    {
        private readonly ILogger<GlobalLogger> _logger;
        private readonly IExternalLogService? _externalLogService;

        public GlobalLogger(ILogger<GlobalLogger> logger, IExternalLogService? externalLogService = null)
        {
            _logger = logger;
            _externalLogService = externalLogService;
        }

        public void LogSystemEvent(string eventType, string message, object? data = null)
        {
            var logEntry = new
            {
                EventType = eventType,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow,
                MachineName = Environment.MachineName
            };

            _logger.LogInformation("[System] {EventType}: {Message} - {@Data}", eventType, message, data);

            try
            {
                _externalLogService?.LogToExternalSystem(logEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send log to external system");
            }
        }

        public void LogBackgroundJob(string jobName, string message, object? data = null)
        {
            var logEntry = new
            {
                JobName = jobName,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("[BackgroundJob] {JobName}: {Message} - {@Data}", jobName, message, data);
            _externalLogService?.LogBackgroundJob(jobName, message, data);
        }

        public void LogIntegrationEvent(string eventName, object payload, bool isSuccess)
        {
            var logLevel = isSuccess ? LogLevel.Information : LogLevel.Error;
            _logger.Log(logLevel, "[Integration] {EventName} - Success: {IsSuccess} - {@Payload}",
                eventName, isSuccess, payload);
        }
    }
}
