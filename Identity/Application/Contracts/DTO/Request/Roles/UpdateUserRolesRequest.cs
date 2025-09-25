using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Contracts.DTO.Request.Roles
{
    public class UpdateUserRolesRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "At least one role is required")]
        [MinLength(1, ErrorMessage = "At least one role is required")]
        public List<string> RoleNames { get; set; } = new List<string>();
    }
}
