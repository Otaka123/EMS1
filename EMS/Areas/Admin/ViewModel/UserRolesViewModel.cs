namespace Identity.API.Areas.Admin.ViewModel
{
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<UserRoleInfo> AllRoles { get; set; } = new List<UserRoleInfo>();
        public List<string> UserCurrentRoles { get; set; } = new List<string>();
    }
}
