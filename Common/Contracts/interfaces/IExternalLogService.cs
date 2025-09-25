using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Application.Contracts.interfaces
{
    public interface IExternalLogService
    {
        void LogToExternalSystem(object logData);
        void LogBackgroundJob(string jobName, string message, object data);
        Task FlushAsync();
    }
}
