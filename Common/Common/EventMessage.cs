using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Application.Common
{
    public class EventMessage
    {
        public string Type { get; set; }
        public object Payload { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
