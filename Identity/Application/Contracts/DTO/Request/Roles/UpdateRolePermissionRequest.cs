using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Contracts.DTO.Request.Roles
{
    /// <summary>
    /// Request DTO for updating role permission
    /// </summary>
    public class UpdateRolePermissionRequest
    {
        public int OldPermissionId { get; set; }
        public int NewPermissionId { get; set; }
    }
}
