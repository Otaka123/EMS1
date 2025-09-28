using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Contracts.DTO.Response.Roles
{
    public class RolePermissionsResponse
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public List<PermissionDTO> CurrentPermissions { get; set; }
        public List<PermissionDTO> AllPermissions { get; set; }
    }
}
