using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Contracts.DTO.Response.Roles
{
    public class PermissionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string PermissionType { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // يمكن إضافة خصائص إضافية إذا لزم الأمر
        public string DisplayName => $"{Category}:{PermissionType}:{Name}";
    }
}
