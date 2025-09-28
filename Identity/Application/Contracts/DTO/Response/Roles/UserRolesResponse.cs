using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Contracts.DTO.Response.Roles
{
    public class UserRolesResponse
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> CurrentRoles { get; set; }
        public List<UserRoleInfo> AllRoles { get; set; }
    }
}
