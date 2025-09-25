using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Application.Contracts.interfaces
{
    public interface IGlobalLogger
    {
        void LogSystemEvent(string eventType, string message, object data = null);
        void LogBackgroundJob(string jobName, string message, object data = null);
        void LogIntegrationEvent(string eventName, object payload, bool isSuccess);
    }
}
