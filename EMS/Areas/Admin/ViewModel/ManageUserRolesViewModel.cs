using Identity.Domain;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Identity.API.Areas.Admin.ViewModel
{
    public class ManageUserRolesViewModel
    {
        public string UserId { get; set; }

        [Display(Name = "اسم المستخدم")]
        public string? UserFullName { get; set; }

        [Display(Name = "البريد الإلكتروني")]
        public string? UserEmail { get; set; }

        [Display(Name = "الأدوار الحالية")]
        public List<string> UserRoles { get; set; } = new List<string>();

        [Display(Name = "اختر الأدوار")]
        public List<string> SelectedRoles { get; set; } = new List<string>(); // تم التصحيح هنا

        public List<ApplicationRole> AllRoles { get; set; } = new List<ApplicationRole>(); // تم التصحيح هنا
    }
}


