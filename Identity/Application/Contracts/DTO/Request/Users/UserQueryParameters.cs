using Identity.Application.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Contracts.DTO.Request.Users
{
    public class UserQueryParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; } = "UserName";
        public string? SortOrder { get; set; } = "asc";
        public bool? IsActive { get; set; }
        public GenderType? Gender { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public DateTime? LastLoginFrom { get; set; }
        public DateTime? LastLoginTo { get; set; }
    }

}
