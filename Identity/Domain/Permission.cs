using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain
{

    public class Permission
    {
        [Key] 
        public int Id { get; set; }
        [Required, MaxLength(150)]
        public string Name { get; set; }
        [MaxLength(200)] 
        public string DisplayName { get; set; }
        [MaxLength(100)] 
        public string Category { get; set; }
        [MaxLength(500)]
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        [MaxLength(100)]
        public string PermissionType { get; set; } 
        // Navigation
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public ICollection<SystemHistory> Histories { get; set; } = new List<SystemHistory>();
        public Permission()
        {
        }

        public Permission(string name, string displayName, string category, string permissionType)
        {
            Name = name;
            DisplayName = displayName;
            Category = category;
            PermissionType = permissionType;
            IsActive = true;
        }
    }
}
