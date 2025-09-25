using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Contracts.DTO.Request.Roles
{
    public class AssignRoleRequest
    {
        public string UserId { get; set; }
        public string RoleName { get; set; }
    }
}

