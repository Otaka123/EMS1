using Identity.Application.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Contracts.DTO.Response.History
{
    public class SystemHistoryDto
    {
        public int Id { get; set; }
        public string EntityName { get; set; }
        public int EntityId { get; set; }
        public ActionType ActionType { get; set; }
        public string ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? Description { get; set; }
        public string? IPAddress { get; set; }

        // يمكن إضافة خصائص مشتقة إذا لزم الأمر
        public string ActionTypeDisplay => ActionType.ToString();
        public string TimeAgo => GetTimeAgo(ChangedAt);

        private string GetTimeAgo(DateTime date)
        {
            var timeSpan = DateTime.UtcNow - date;
            if (timeSpan.TotalDays > 365)
                return $"{(int)(timeSpan.TotalDays / 365)} years ago";
            if (timeSpan.TotalDays > 30)
                return $"{(int)(timeSpan.TotalDays / 30)} months ago";
            if (timeSpan.TotalDays > 1)
                return $"{(int)timeSpan.TotalDays} days ago";
            if (timeSpan.TotalHours > 1)
                return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalMinutes > 1)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";

            return "Just now";
        }
    }
}
