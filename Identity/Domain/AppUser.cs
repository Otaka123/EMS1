using Identity.Application.Contracts.Enum;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain
{
    public class AppUser : IdentityUser
    {

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        [Column(TypeName = "date")]
        public DateTime? DOB { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? ProfilePictureUrl { get; set; }
        public string? Address { get; set; }

        public GenderType Gender { get; set; } = GenderType.Unknown;
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginDate { get; set; }

        public void UpdateProfilePicture(string? profilePicture)
        {
            ProfilePictureUrl = profilePicture;
        }


    }
}
