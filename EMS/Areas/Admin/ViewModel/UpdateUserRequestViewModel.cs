using System.ComponentModel.DataAnnotations;

namespace Identity.API.Areas.Admin.ViewModel
{
    public class UpdateUserRequestViewModel
    {
        [Required(ErrorMessage = "معرف المستخدم مطلوب")]
        public string UserId { get; set; }

        [Display(Name = "الاسم الأول")]
        [StringLength(50, ErrorMessage = "الاسم الأول يجب ألا يتجاوز 50 حرفاً")]
        public string FirstName { get; set; }

        [Display(Name = "الاسم الأخير")]
        [StringLength(50, ErrorMessage = "الاسم الأخير يجب ألا يتجاوز 50 حرفاً")]
        public string LastName { get; set; }



        [Display(Name = "كلمة المرور الحالية")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

    }

}
