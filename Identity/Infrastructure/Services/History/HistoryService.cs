using AutoMapper;
using Common.Application.Common;
using Common.Application.Contracts.interfaces;
using Identity.Application.Contracts.DTO.Request.History;
using Identity.Application.Contracts.DTO.Response.History;
using Identity.Application.Contracts.Enum;
using Identity.Application.Contracts.Interfaces;
using Identity.Domain;
using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Services.History
{
    public class HistoryService : IHistoryService
    {
        private readonly AppIdentityDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<HistoryService> _logger;
        private readonly IMapper _mapper;

        public HistoryService(
            AppIdentityDbContext dbContext,
            ICurrentUserService currentUserService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<HistoryService> logger,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task RecordHistoryAsync<TEntity>(int entityId, ActionType actionType,
                                                    string? oldValues = null, string? newValues = null,
                                                    string? description = null, string? ipAddress = null)
                                                    where TEntity : class
        {
            try
            {
                var entityName = typeof(TEntity).Name;
                var userId = _currentUserService.UserId ?? "System";
                ipAddress ??= GetClientIPAddress();

                var historyEntry = new SystemHistory(
                    entityName: entityName,
                    entityId: entityId,
                    actionType: actionType,
                    changedByUserId: userId,
                    oldValues: oldValues,
                    newValues: newValues,
                    description: description,
                    ipAddress: ipAddress
                );

                await _dbContext.SystemHistories.AddAsync(historyEntry);
                await _dbContext.SaveChangesAsync();

                _logger.LogDebug("History recorded for {EntityName} {EntityId}, Action: {ActionType}",
                    entityName, entityId, actionType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record history for {EntityName} {EntityId}, Action: {ActionType}",
                    typeof(TEntity).Name, entityId, actionType);
                // يمكنك اختيار whether to throw or log only based on your requirements
                throw new ApplicationException($"Failed to record history: {ex.Message}");
            }
        }

        public async Task RecordHistoryAsync<TEntity>(int entityId, ActionType actionType,
                                                    object oldEntity = null, object newEntity = null,
                                                    string? description = null, string? ipAddress = null)
                                                    where TEntity : class
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string oldValues = oldEntity != null ? JsonSerializer.Serialize(oldEntity, options) : null;
            string newValues = newEntity != null ? JsonSerializer.Serialize(newEntity, options) : null;

            await RecordHistoryAsync<TEntity>(entityId, actionType, oldValues, newValues, description, ipAddress);
        }

        public async Task<List<SystemHistory>> GetEntityHistoryAsync<TEntity>(int entityId,
                                                                            CancellationToken cancellationToken = default)
                                                                            where TEntity : class
        {
            var entityName = typeof(TEntity).Name;

            return await _dbContext.SystemHistories
                .Where(h => h.EntityName == entityName && h.EntityId == entityId)
                .OrderByDescending(h => h.ChangedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SystemHistory>> GetHistoryByEntityTypeAsync<TEntity>(
            CancellationToken cancellationToken = default) where TEntity : class
        {
            var entityName = typeof(TEntity).Name;

            return await _dbContext.SystemHistories
                .Where(h => h.EntityName == entityName)
                .OrderByDescending(h => h.ChangedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SystemHistory>> GetHistoryByDateRangeAsync(DateTime fromDate, DateTime toDate,
                                                                        CancellationToken cancellationToken = default)
        {
            return await _dbContext.SystemHistories
                .Where(h => h.ChangedAt >= fromDate && h.ChangedAt <= toDate)
                .OrderByDescending(h => h.ChangedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SystemHistory>> GetHistoryByUserAsync(string userId,
                                                                   CancellationToken cancellationToken = default)
        {
            return await _dbContext.SystemHistories
                .Where(h => h.ChangedByUserId == userId)
                .OrderByDescending(h => h.ChangedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SystemHistory>> GetHistoryByActionTypeAsync(ActionType actionType,
                                                                         CancellationToken cancellationToken = default)
        {
            return await _dbContext.SystemHistories
                .Where(h => h.ActionType == actionType)
                .OrderByDescending(h => h.ChangedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedResponseDto<SystemHistory>> SearchHistoryAsync(HistorySearchRequestDto filter,
                                                                        CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _dbContext.SystemHistories.AsQueryable();

                // تطبيق الفلاتر الأساسية
                if (!string.IsNullOrEmpty(filter.EntityName))
                    query = query.Where(h => h.EntityName == filter.EntityName);

                if (filter.EntityId.HasValue)
                    query = query.Where(h => h.EntityId == filter.EntityId.Value);

                if (filter.ActionType.HasValue)
                    query = query.Where(h => h.ActionType == filter.ActionType.Value);

                if (!string.IsNullOrEmpty(filter.UserId))
                    query = query.Where(h => h.ChangedByUserId == filter.UserId);

                if (filter.FromDate.HasValue)
                    query = query.Where(h => h.ChangedAt >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(h => h.ChangedAt <= filter.ToDate.Value);

                if (!string.IsNullOrEmpty(filter.DescriptionContains))
                    query = query.Where(h => h.Description != null &&
                                           h.Description.Contains(filter.DescriptionContains));

                // البحث العام (SearchTerm) - يبحث في multiple fields
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.ToLower();
                    query = query.Where(h =>
                        h.EntityName.ToLower().Contains(searchTerm) ||
                        (h.Description != null && h.Description.ToLower().Contains(searchTerm)) ||
                        h.ChangedByUserId.ToLower().Contains(searchTerm) ||
                        (h.OldValues != null && h.OldValues.ToLower().Contains(searchTerm)) ||
                        (h.NewValues != null && h.NewValues.ToLower().Contains(searchTerm))
                    );
                }

                // الحصول على العدد الإجمالي قبل التصفح
                var totalCount = await query.CountAsync(cancellationToken);

                // التصنيف (Sorting)
                query = ApplySorting(query, filter.SortBy, filter.SortOrder);

                // التصفح (Pagination)
                var items = await query
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                return new PagedResponseDto<SystemHistory>(
                    items: items,
                    totalCount: totalCount,
                    pageNumber: filter.PageNumber,
                    pageSize: filter.PageSize,
                    searchTerm: filter.SearchTerm,
                    sortBy: filter.SortBy,
                    sortOrder: filter.SortOrder
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search history with filter: {@Filter}", filter);
                throw new ApplicationException($"Failed to search history: {ex.Message}");
            }
        }

        private IQueryable<SystemHistory> ApplySorting(IQueryable<SystemHistory> query, string? sortBy, string? sortOrder)
        {
            sortBy = string.IsNullOrEmpty(sortBy) ? "ChangedAt" : sortBy;
            sortOrder = string.IsNullOrEmpty(sortOrder) ? "desc" : sortOrder.ToLower();

            return (sortBy.ToLower(), sortOrder) switch
            {
                ("entityname", "asc") => query.OrderBy(h => h.EntityName),
                ("entityname", "desc") => query.OrderByDescending(h => h.EntityName),
                ("entityid", "asc") => query.OrderBy(h => h.EntityId),
                ("entityid", "desc") => query.OrderByDescending(h => h.EntityId),
                ("actiontype", "asc") => query.OrderBy(h => h.ActionType),
                ("actiontype", "desc") => query.OrderByDescending(h => h.ActionType),
                ("changedby", "asc") => query.OrderBy(h => h.ChangedByUserId),
                ("changedby", "desc") => query.OrderByDescending(h => h.ChangedByUserId),
                ("changedat", "asc") => query.OrderBy(h => h.ChangedAt),
                ("changedat", "desc") => query.OrderByDescending(h => h.ChangedAt),
                ("description", "asc") => query.OrderBy(h => h.Description),
                ("description", "desc") => query.OrderByDescending(h => h.Description),
                _ => query.OrderByDescending(h => h.ChangedAt) // Default sorting
            };
        }

        public async Task<int> CleanupOldHistoryAsync(DateTime olderThan,
                                                    CancellationToken cancellationToken = default)
        {
            try
            {
                var oldRecords = _dbContext.SystemHistories
                    .Where(h => h.ChangedAt < olderThan);

                int deletedCount = await oldRecords.ExecuteDeleteAsync(cancellationToken);

                _logger.LogInformation("Cleaned up {Count} old history records older than {OlderThan}",
                    deletedCount, olderThan);

                return deletedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup old history records older than {OlderThan}", olderThan);
                throw;
            }
        }

        private string? GetClientIPAddress()
        {
            try
            {
                return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            }
            catch
            {
                return null;
            }
        }

        // 🔹 دالة مساعدة لإنشاء وصف تلقائي
        public string GenerateActionDescription<TEntity>(ActionType actionType, int entityId)
        {
            var entityName = typeof(TEntity).Name;
            return actionType switch
            {
                ActionType.Created => $"{entityName} with ID {entityId} was created",
                ActionType.Updated => $"{entityName} with ID {entityId} was updated",
                ActionType.Deleted => $"{entityName} with ID {entityId} was deleted",
                ActionType.read => $"{entityName} with ID {entityId} was viewed",
                _ => $"{actionType} performed on {entityName} with ID {entityId}"
            };
        }
    }
}
