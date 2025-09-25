using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Contracts.DTO.Common
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;

        // إضافة مفيدة للتنقل بين الصفحات
        public int? NextPageNumber => HasNext ? PageNumber + 1 : null;
        public int? PreviousPageNumber => HasPrevious ? PageNumber - 1 : null;

        // إضافة لضمان عدم وجود قيم غير منطقية
        public PagedResult()
        {
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 10; // أو قيمة افتراضية معقولة
        }
    }
}
