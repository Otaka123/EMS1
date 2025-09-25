using Common.Application.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Application.Common
{
    public class NotificationEvent
    {
        public string EventId { get; } = Guid.NewGuid().ToString();
        public string EventType { get; set; } // مثال: "Order.Created", "Payment.Processed"
        public string RecipientId { get; set; }
        public string TemplateId { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
        public List<NotificationChannel> Channels { get; set; } // Email, SMS, Push, etc.
    }
}
