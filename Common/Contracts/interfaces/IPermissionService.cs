using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Application.Contracts.interfaces
{
    public interface IPermissionService
    {
        /// <summary>
        /// التحقق من صلاحية محددة للمستخدم
        /// </summary>
        Task<bool> HasPermissionAsync(string userId, string permission);

        /// <summary>
        /// التحقق من وجود أي من الصلاحيات المطلوبة
        /// </summary>
        Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissions);

        /// <summary>
        /// التحقق من وجود كل الصلاحيات المطلوبة
        /// </summary>
        Task<bool> HasAllPermissionsAsync(string userId, IEnumerable<string> permissions);

        /// <summary>
        /// التحقق من وجود أي من الأدوار المطلوبة
        /// </summary>
        Task<bool> HasAnyRoleAsync(string userId, IEnumerable<string> roles);

        /// <summary>
        /// جلب جميع صلاحيات المستخدم
        /// </summary>
        Task<IEnumerable<string>> GetUserPermissionsAsync(string userId);

        /// <summary>
        /// جلب جميع أدوار المستخدم
        /// </summary>
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);
    }
}
