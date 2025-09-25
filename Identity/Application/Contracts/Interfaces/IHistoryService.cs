using Common.Application.Common;
using Identity.Application.Contracts.DTO.Request.History;
using Identity.Application.Contracts.DTO.Response.History;
using Identity.Application.Contracts.Enum;
using Identity.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Contracts.Interfaces
{
    public interface IHistoryService
    {
        Task RecordHistoryAsync<TEntity>(int entityId, ActionType actionType,
                                          string? oldValues = null, string? newValues = null,
                                          string? description = null, string? ipAddress = null)
                                          where TEntity : class;

        // التسجيل مع الكائنات مباشرة (Auto Serialization)
        Task RecordHistoryAsync<TEntity>(int entityId, ActionType actionType,
                                       object oldEntity = null, object newEntity = null,
                                       string? description = null, string? ipAddress = null)
                                       where TEntity : class;

        // استعلامات التاريخ
        Task<List<SystemHistory>> GetEntityHistoryAsync<TEntity>(int entityId,
                                                                CancellationToken cancellationToken = default)
                                                                where TEntity : class;

        Task<List<SystemHistory>> GetHistoryByEntityTypeAsync<TEntity>(
            CancellationToken cancellationToken = default) where TEntity : class;

        Task<List<SystemHistory>> GetHistoryByDateRangeAsync(DateTime fromDate, DateTime toDate,
                                                           CancellationToken cancellationToken = default);

        Task<List<SystemHistory>> GetHistoryByUserAsync(string userId,
                                                      CancellationToken cancellationToken = default);

        Task<List<SystemHistory>> GetHistoryByActionTypeAsync(ActionType actionType,
                                                            CancellationToken cancellationToken = default);

        // البحث والترشيح المتقدم باستخدام PagedResponseDto
        Task<PagedResponseDto<SystemHistory>> SearchHistoryAsync(HistorySearchRequestDto filter,
                                                               CancellationToken cancellationToken = default);

        // الصيانة
        Task<int> CleanupOldHistoryAsync(DateTime olderThan,
                                       CancellationToken cancellationToken = default);
    }


}
