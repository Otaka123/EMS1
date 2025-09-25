using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Contracts.DTO.Request.Roles
{
    public class EditRoleRequest
    {
        public string RoleId { get; set; }
        public string NewRoleName { get; set; }
    }
}
