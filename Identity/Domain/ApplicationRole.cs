using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain
{
    public class ApplicationRole : IdentityRole
    {
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public ApplicationRole() { } // Default constructor

        public ApplicationRole(string roleName) : base(roleName)
        {
        }
    }
}
