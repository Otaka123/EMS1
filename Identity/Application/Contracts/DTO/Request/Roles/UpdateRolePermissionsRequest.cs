using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Contracts.DTO.Request.Roles
{
    public class UpdateRolePermissionsRequest
    {
        public string RoleId { get; set; }
        public List<int> SelectedPermissionIds { get; set; } = new List<int>();
    }
}
