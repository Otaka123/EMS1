using Identity.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Contracts.DTO.Response.Roles
{
    public class ManageUserRolesDTO
            {
        public string UserId { get; set; }

        public string? UserFullName { get; set; }

        public string? UserEmail { get; set; }

        public List<string> UserRoles { get; set; } = new List<string>();

        public List<string> SelectedRoles { get; set; } = new List<string>(); // تم التصحيح هنا

        public List<ApplicationRole> AllRoles { get; set; } = new List<ApplicationRole>(); // تم التصحيح
    }
}
