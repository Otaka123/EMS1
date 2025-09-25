using Common.Application.Contracts.interfaces;
using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.infrastructure.Services.Logger
{

    public class AppInsightsLogService : IExternalLogService
    {
        private readonly TelemetryClient _telemetryClient;

        public AppInsightsLogService(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        public void LogToExternalSystem(object logData)
        {
            try
            {
                if (logData == null) return;

                var properties = ConvertToDictionary(logData);
                _telemetryClient.TrackEvent("SystemEvent", properties);
            }
            catch (Exception ex)
            {
                // Fallback logging
                _telemetryClient.TrackException(ex, new Dictionary<string, string>
                {
                    ["OriginalData"] = logData?.ToString() ?? "null",
                    ["Error"] = "Failed to log system event"
                });
            }
        }

        private static Dictionary<string, string> ConvertToDictionary(object data)
        {
            if (data is Dictionary<string, string> dict)
                return dict;

            return data.GetType()
                .GetProperties()
                .Where(p => p.CanRead)
                .ToDictionary(
                    p => p.Name,
                    p => p.GetValue(data)?.ToString() ?? "null"
                );
        }

        public void LogBackgroundJob(string jobName, string message, object data)
        {
            var properties = new Dictionary<string, string>
            {
                ["JobName"] = jobName,
                ["Message"] = message
            };

            if (data != null)
            {
                foreach (var prop in ConvertToDictionary(data))
                {
                    properties[$"Data_{prop.Key}"] = prop.Value;
                }
            }

            _telemetryClient.TrackEvent("BackgroundJob", properties);
        }

        public async Task FlushAsync()
        {
            _telemetryClient.Flush();
            await Task.Delay(1000); // Allow time for flushing
        }

    }
}
