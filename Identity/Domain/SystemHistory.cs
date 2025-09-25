using Identity.Application.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain
{
    public class SystemHistory
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string EntityName { get; set; }  // اسم الكيان (مثل: Permission, User, Role)

        [Required]
        public int EntityId { get; set; }      // ID الكيان

        [Required]
        public ActionType ActionType { get; set; }

        [Required, MaxLength(100)]
        public string ChangedByUserId { get; set; }

        [Required]
        public DateTime ChangedAt { get; set; }

        public string? OldValues { get; set; }  // JSON للقيم القديمة
        public string? NewValues { get; set; }  // JSON للقيم الجديدة

        [MaxLength(500)]
        public string? Description { get; set; }  // وصف للتغيير

        [MaxLength(50)]
        public string? IPAddress { get; set; }    // عنوان IP للمستخدم

        // ✅ Constructor فارغ مطلوب لـ EF Core
        public SystemHistory() { }

        // ✅ Constructor مخصص
        public SystemHistory(string entityName, int entityId, ActionType actionType,
                           string changedByUserId, string? oldValues = null,
                           string? newValues = null, string? description = null,
                           string? ipAddress = null)
        {
            EntityName = entityName;
            EntityId = entityId;
            ActionType = actionType;
            ChangedByUserId = changedByUserId;
            ChangedAt = DateTime.UtcNow;
            OldValues = oldValues;
            NewValues = newValues;
            Description = description;
            IPAddress = ipAddress;
        }
    }

}
