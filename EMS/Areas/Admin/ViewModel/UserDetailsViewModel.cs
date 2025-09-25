using Identity.Application.Contracts.DTO.Response.User;

namespace Identity.API.Areas.Admin.ViewModel
{
    public class UserDetailsViewModel
    {
        public UserResponse User { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

}
