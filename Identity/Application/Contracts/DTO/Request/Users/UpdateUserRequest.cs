using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Contracts.DTO.Request.User
{
    public class UpdateUserRequest
    {

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DOB { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Address { get; set; }
        public string CurrentPassword { get; set; }
        //public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        //public string? UserName { get; set; }

    }
}
