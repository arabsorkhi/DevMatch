using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Common
{
    public sealed record PagedResult<T>(
        IReadOnlyCollection<T> Items,
        int PageNumber,
        int PageSize,
        int TotalCount,
        int TotalPages,
        bool HasPreviousPage,
        bool HasNextPage)
    {
        public static PagedResult<T> Create(
            IReadOnlyCollection<T> items,
            int pageNumber,
            int pageSize,
            int totalCount)
        {
            int totalPages = totalCount == 0
                ? 0
                : (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResult<T>(
                items,
                pageNumber,
                pageSize,
                totalCount,
                totalPages,
                pageNumber > 1,
                pageNumber < totalPages);
        }
    }

}
