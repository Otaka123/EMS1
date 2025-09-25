using Identity.Application.Contracts.DTO.Response.Roles;
using Identity.Application.Contracts.DTO.Response.User;

namespace Identity.API.Areas.Admin.ViewModel
{
    public class RolePermissionsViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public List<PermissionDTO>? CurrentPermissions { get; set; }
        public List<PermissionDTO>? AllPermissions { get; set; }
        public List<UserInRoleDTO> UsersInRole { get; set; } = new List<UserInRoleDTO>();
        public List<UserInRoleDTO> AllUsers { get; set; } = new List<UserInRoleDTO>();
        public List<string> SelectedUserIds { get; set; } = new List<string>();

        public List<int> SelectedPermissionIds { get; set; } = new List<int>();
    }
}
