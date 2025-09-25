using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Contracts.DTO.Request.User
{
    public class RegisterRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? DOB { get; set; }
        public string? UserName { get; set; }
        public string? Phone { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public string ConfirmPassword { get; set; } = string.Empty;
    }

}
