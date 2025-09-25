using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Contracts.DTO.Response.User
{
    public class UserResponse
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? FullName => $"{FirstName} {LastName}".Trim();
        public string Email { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; } // سيتم تحويلها من enum إلى string
        public int? Age => CalculateAge();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Address { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; } = true;


        private int? CalculateAge()
        {
            if (!DateOfBirth.HasValue) return null;

            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Value.Year;
            if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}
