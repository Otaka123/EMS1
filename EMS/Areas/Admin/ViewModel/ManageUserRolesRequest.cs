namespace Identity.API.Areas.Admin.ViewModel
{
    public class ManageUserRolesRequest
    {
        public string UserId { get; set; }
        public List<string> SelectedRoleIds { get; set; } = new List<string>();
    }
}
