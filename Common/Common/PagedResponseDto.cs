using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Application.Common
{
    public record PagedResponseDto<T>
    {
        public List<T> Items { get; init; } = new();
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public string? SearchTerm { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public PagedResponseDto() { }

        public PagedResponseDto(List<T> items, int totalCount, int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, string? sortOrder = null)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            SearchTerm = searchTerm;
            SortBy = sortBy;
            SortOrder = sortOrder;
        }
    }

}
