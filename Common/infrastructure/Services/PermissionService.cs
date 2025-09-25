//using Common.Application.Contracts.interfaces;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Common.infrastructure.Services
//{
//    public class PermissionService : IPermissionService
//    {
//        private readonly  _context;
//        private readonly IUserCacheService _userCache;

//        public PermissionService(
//            ApplicationDbContext context,
//            IUserCacheService userCache)
//        {
//            _context = context;
//            _userCache = userCache;
//        }

//        public async Task<bool> HasPermissionAsync(string userId, string permission)
//        {
//            // التحقق من الذاكرة المؤقتة أولاً
//            var cachedPermissions = await _userCache.GetUserPermissionsAsync(userId);
//            if (cachedPermissions.Contains(permission))
//                return true;

//            // إذا لم توجد في الذاكرة المؤقتة، البحث في قاعدة البيانات
//            var hasPermission = await _context.UserPermissions
//                .AnyAsync(up => up.UserId == userId && up.Permission.Name == permission);

//            // تحديث الذاكرة المؤقتة إذا لزم الأمر
//            if (hasPermission)
//                await _userCache.AddPermissionToUserAsync(userId, permission);

//            return hasPermission;
//        }

//        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissions)
//        {
//            var userPermissions = await GetUserPermissionsAsync(userId);
//            return permissions.Any(p => userPermissions.Contains(p));
//        }

//        public async Task<bool> HasAllPermissionsAsync(string userId, IEnumerable<string> permissions)
//        {
//            var userPermissions = await GetUserPermissionsAsync(userId);
//            return permissions.All(p => userPermissions.Contains(p));
//        }

//        public async Task<bool> HasAnyRoleAsync(string userId, IEnumerable<string> roles)
//        {
//            var userRoles = await GetUserRolesAsync(userId);
//            return roles.Any(r => userRoles.Contains(r));
//        }

//        public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userId)
//        {
//            // الجمع بين الصلاحيات المباشرة وصلاحيات الأدوار
//            var directPermissions = await _context.UserPermissions
//                .Where(up => up.UserId == userId)
//                .Select(up => up.Permission.Name)
//                .ToListAsync();

//            var rolePermissions = await _context.UserRoles
//                .Where(ur => ur.UserId == userId)
//                .SelectMany(ur => ur.Role.Permissions.Select(rp => rp.Permission.Name))
//                .ToListAsync();

//            return directPermissions.Union(rolePermissions).Distinct();
//        }

//        public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
//        {
//            return await _context.UserRoles
//                .Where(ur => ur.UserId == userId)
//                .Select(ur => ur.Role.Name)
//                .ToListAsync();
//        }
//    }
//}
