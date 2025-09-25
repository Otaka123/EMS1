using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Application.Contracts.interfaces
{
    public interface IAppLogger<out T> where T : class
    {
        // التسجيل الأساسي
        void LogTrace(string message, params object[] args);
        void LogDebug(string message, params object[] args);
        void LogInformation(string message, params object[] args);

        void LogWarning(string message, params object[] args);
        void LogWarning(Exception ex, string message, params object[] args);

        void LogError(string message, params object[] args);    
        void LogError(Exception ex, string message, params object[] args);
        void LogCritical(string message, params object[] args);

        // التسجيل المعزز
        void LogPerformance(string operationName, Action operation);
        void LogPerformance(string operationName, long milliseconds);

        Task LogPerformanceAsync(string operationName, Func<Task> operation);
        void LogRequestResponse(object request, object response, TimeSpan duration);
        void LogValidationErrors(IEnumerable<ValidationFailure> errors);

        // تسجيل الاستثناءات
        void LogException(Exception ex, string? message = null, params object[] args);
        void LogSecurityEvent(string eventType, string message, object? additionalData=null );
    }
}
