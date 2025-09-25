using Common.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Application.Contracts.interfaces
{
    public interface IEventService
    {
        bool Publish(string eventType, object payload);
        Task<bool> PublishAsync(string eventType, object payload);
        bool Subscribe(string eventType, Func<EventMessage, Task> handler);
        bool Unsubscribe(string eventType, Func<EventMessage, Task> handler);
    }
}
