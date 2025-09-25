//using AutoMapper;
//using Common.Application.Contracts.interfaces;
//using Identity.Application.Contracts.Enum;
//using Identity.Application.Contracts.Interfaces;
//using Identity.Domain;
//using Identity.Infrastructure.Persistence;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Identity.Infrastructure.Services.History
//{
//    public class PermissionHistoryService : IPermissionHistoryService
//    {
//        private readonly AppIdentityDbContext _dbContext;
//        private readonly ICurrentUserService _currentUserService;
//        private readonly ILogger<PermissionHistoryService> _logger;
//        private readonly IMapper _mapper;

//        public PermissionHistoryService(
//            AppIdentityDbContext dbContext,
//            ICurrentUserService currentUserService,
//            ILogger<PermissionHistoryService> logger,
//            IMapper mapper)
//        {
//            _dbContext = dbContext;
//            _currentUserService = currentUserService;
//            _logger = logger;
//            _mapper = mapper;
//        }

//        public async Task RecordHistoryAsync(int permissionId, ActionType actionType, string createdByUserId,
//                                           string? oldValue = null, string? newValue = null)
//        {
//            try
//            {
//                var historyEntry = new PermissionHistory(
//                    permissionId: permissionId,
//                    actionType: actionType,
//                    createdByUserId: createdByUserId,
//                    changedAt: DateTime.UtcNow,
//                    oldValue: oldValue,
//                    newValue: newValue
//                );

//                await _dbContext.PermissionHistories.AddAsync(historyEntry);
//                await _dbContext.SaveChangesAsync();

//                _logger.LogInformation("History recorded for Permission {PermissionId}, Action: {ActionType}",
//                    permissionId, actionType);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to record history for Permission {PermissionId}, Action: {ActionType}",
//                    permissionId, actionType);
//                throw; // أو التعامل مع الخطأ حسب متطلباتك
//            }
//        }

//        public async Task RecordHistoryAsync(int permissionId, ActionType actionType,
//                                           string? oldValue = null, string? newValue = null)
//        {
//            var currentUserId = _currentUserService.UserId ?? "System";
//            await RecordHistoryAsync(permissionId, actionType, currentUserId, oldValue, newValue);
//        }

//        // 🔹 دالة مساعدة لتسجيل التاريخ مع كائنات بدلاً من JSON
//        public async Task RecordHistoryWithObjectsAsync<T>(int permissionId, ActionType actionType,
//                                                         T oldObject = null, T newObject = null) where T : class
//        {
//            string oldValueJson = oldObject != null ? JsonSerializer.Serialize(oldObject) : null;
//            string newValueJson = newObject != null ? JsonSerializer.Serialize(newObject) : null;

//            await RecordHistoryAsync(permissionId, actionType, oldValueJson, newValueJson);
//        }

//        // 🔹 الحصول على تاريخ التغييرات لـ Permission معين
//        public async Task<List<PermissionHistory>> GetPermissionHistoryAsync(int permissionId,
//                                                                           CancellationToken cancellationToken = default)
//        {
//            try
//            {
//                return await _dbContext.PermissionHistories
//                    .Where(ph => ph.PermissionId == permissionId)
//                    .OrderByDescending(ph => ph.ChangedAt)
//                    .AsNoTracking()
//                    .ToListAsync(cancellationToken);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to get history for Permission {PermissionId}", permissionId);
//                throw;
//            }
//        }

//        // 🔹 الحصول على التغييرات خلال فترة زمنية
//        public async Task<List<PermissionHistory>> GetHistoryByDateRangeAsync(DateTime fromDate, DateTime toDate,
//                                                                             CancellationToken cancellationToken = default)
//        {
//            try
//            {
//                return await _dbContext.PermissionHistories
//                    .Where(ph => ph.ChangedAt >= fromDate && ph.ChangedAt <= toDate)
//                    .OrderByDescending(ph => ph.ChangedAt)
//                    .Include(ph => ph.Permission) // إذا أردت تضمين بيانات الـ Permission
//                    .AsNoTracking()
//                    .ToListAsync(cancellationToken);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to get history from {FromDate} to {ToDate}", fromDate, toDate);
//                throw;
//            }
//        }

//        // 🔹 الحصول على التغييرات بواسطة مستخدم معين
//        public async Task<List<PermissionHistory>> GetHistoryByUserAsync(string userId,
//                                                                        CancellationToken cancellationToken = default)
//        {
//            try
//            {
//                return await _dbContext.PermissionHistories
//                    .Where(ph => ph.CreateByUserID == userId)
//                    .OrderByDescending(ph => ph.ChangedAt)
//                    .Include(ph => ph.Permission)
//                    .AsNoTracking()
//                    .ToListAsync(cancellationToken);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to get history for User {UserId}", userId);
//                throw;
//            }
//        }

//        // 🔹 حذف التاريخ الأقدم من تاريخ معين (لأغراض الصيانة)
//        public async Task<int> CleanupOldHistoryAsync(DateTime olderThan,
//                                                     CancellationToken cancellationToken = default)
//        {
//            try
//            {
//                var oldRecords = _dbContext.PermissionHistories
//                    .Where(ph => ph.ChangedAt < olderThan);

//                int deletedCount = await oldRecords.ExecuteDeleteAsync(cancellationToken);

//                _logger.LogInformation("Cleaned up {Count} old history records older than {OlderThan}",
//                    deletedCount, olderThan);

//                return deletedCount;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to cleanup old history records older than {OlderThan}", olderThan);
//                throw;
//            }
//        }
//    }
//}
