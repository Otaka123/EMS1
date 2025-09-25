using Common.Application.Contracts.interfaces;
using Common.Application.Contracts.Interfaces;
using Identity.Application.Contracts.DTO.Request.Roles;
using Identity.Application.Contracts.DTO.Response.Roles;
using Identity.Application.Contracts.Interfaces;
using Identity.Contracts.DTO.Request.Roles;
using Identity.Domain;
using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Services.Roles
{
    public class RoleService : IRoleService
    {
        //private readonly IRoleCacheService _roleCache;

        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<RoleService> _logger;
        private readonly ISharedMessageLocalizer _localizer;
        private readonly IValidationService _validationService;
        private readonly AppIdentityDbContext _dbContext;
        public RoleService(
            RoleManager<ApplicationRole> roleManager,
            UserManager<AppUser> userManager,
            ILogger<RoleService> logger,
            AppIdentityDbContext dbContext,
             ISharedMessageLocalizer localizer,
             IValidationService validationService
           )
        {
            _dbContext = dbContext;
            _localizer = localizer;
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
            _validationService = validationService;
        }

        // الحصول على جميع الأدوار
        public async Task<RequestResponse<IEnumerable<ApplicationRole>>> GetAllRolesAsync(
           CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var roles = await _roleManager.Roles.ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved all roles successfully");
                return RequestResponse<IEnumerable<ApplicationRole>>.Ok(
                    data: roles,
                    message: _localizer["Roles.RetrievedSuccessfully"]
                );
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("GetAllRoles operation was cancelled");
                return RequestResponse<IEnumerable<ApplicationRole>>.Fail("Operation was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving all roles");
                return RequestResponse<IEnumerable<ApplicationRole>>.InternalServerError(_localizer["SystemError"]);
            }
        }


        // إنشاء دور جديد

        public async Task<RequestResponse<ApplicationRole>> AddNewRoleAsync(
         AddRoleRequest request,
         CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return RequestResponse<ApplicationRole>.BadRequest(
                        errors: validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                }

                //var existingRoles = await _roleCache.GetValidRolesAsync();
                //if (existingRoles.Contains(request.RoleName))
                //{
                //    _logger.LogWarning(_localizer["Roles.RoleAlreadyExists"], request.RoleName);
                //    return RequestResponse<ApplicationRole>.BadRequest(_localizer["Roles.RoleAlreadyExists"]);
                //}

                cancellationToken.ThrowIfCancellationRequested();

                var role = new ApplicationRole(request.RoleName);
                var result = await _roleManager.CreateAsync(role);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning(_localizer["Roles.CreateRoleFailed"],
                        request.RoleName, string.Join(", ", errors));
                    return RequestResponse<ApplicationRole>.Fail("", errors);
                }

                //await _roleCache.RefreshCacheAsync();

                _logger.LogInformation(_localizer["Roles.CreatedSuccessfully"], request.RoleName);
                return RequestResponse<ApplicationRole>.Ok(
                    data: role,
                    message: _localizer["Roles.CreatedSuccessfully"]
                );
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(_localizer["OperationCancelled"], request?.RoleName);
                return RequestResponse<ApplicationRole>.Fail(_localizer["OperationCancelled"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer["Roles.SystemError"], request?.RoleName);
                return RequestResponse<ApplicationRole>.InternalServerError(_localizer["Roles.SystemError"]);
            }
        }




        public async Task<RequestResponse<bool>> AssignRoleToUserAsync(
      AssignRoleRequest request,
      CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return RequestResponse<bool>.BadRequest(
                        errors: validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                }

                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning(_localizer["Roles.UserNotFound"], request.UserId);
                    return RequestResponse<bool>.NotFound(_localizer["Roles.UserNotFound"]);
                }

                var validRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync(cancellationToken);
                if (!validRoles.Contains(request.RoleName))
                {
                    _logger.LogWarning(_localizer["Roles.RoleNotFound"], request.RoleName);
                    return RequestResponse<bool>.NotFound(_localizer["Roles.RoleNotFound"]);
                }

                cancellationToken.ThrowIfCancellationRequested();

                if (await _userManager.IsInRoleAsync(user, request.RoleName))
                {
                    _logger.LogWarning(_localizer["Roles.UserAlreadyHasRole"], request.UserId, request.RoleName);
                    return RequestResponse<bool>.BadRequest(_localizer["Roles.UserAlreadyHasRole"]);
                }

                var result = await _userManager.AddToRoleAsync(user, request.RoleName);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning(_localizer["Roles.AssignRoleFailed"],
                        request.RoleName, request.UserId, string.Join(", ", errors));
                    return RequestResponse<bool>.Fail("", errors);
                }

                _logger.LogInformation(_localizer["Roles.AssignedSuccessfully"],
                    request.RoleName, request.UserId);
                return RequestResponse<bool>.Ok(
                    data: true,
                    message: _localizer["Roles.AssignedSuccessfully"]
                );
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(_localizer["OperationCancelled"], request?.UserId);
                return RequestResponse<bool>.Fail(_localizer["OperationCancelled"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer["Roles.SystemError"],
                    request?.RoleName, request?.UserId);
                return RequestResponse<bool>.InternalServerError(_localizer["Roles.SystemError"]);
            }
        }

        // تحديث اسم الدور
        public async Task<RequestResponse<ApplicationRole>> EditRoleAsync(
       EditRoleRequest request,
       CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return RequestResponse<ApplicationRole>.BadRequest(
                        errors: validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                }

                var role = await _roleManager.FindByIdAsync(request.RoleId);
                if (role == null)
                {
                    _logger.LogWarning(_localizer["Roles.RoleNotFound"], request.RoleId);
                    return RequestResponse<ApplicationRole>.NotFound(_localizer["Roles.RoleNotFound"]);
                }
                var existingRoles = await _roleManager.Roles.Select(s=> s.Name).ToListAsync(cancellationToken);

                if (existingRoles.Contains(request.NewRoleName) && request.NewRoleName != role.Name)
                {
                    _logger.LogWarning(_localizer["Roles.RoleAlreadyExists"], request.NewRoleName);
                    return RequestResponse<ApplicationRole>.BadRequest(_localizer["Roles.RoleAlreadyExists"]);
                }

                cancellationToken.ThrowIfCancellationRequested();

                role.Name = request.NewRoleName;
                var result = await _roleManager.UpdateAsync(role);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning(_localizer["Roles.UpdateRoleFailed"],
                        request.RoleId, string.Join(", ", errors));
                    return RequestResponse<ApplicationRole>.Fail("Fail", errors);
                }


                _logger.LogInformation(_localizer["Roles.UpdatedSuccessfully"], request.RoleId);
                return RequestResponse<ApplicationRole>.Ok(
                    data: role,
                    message: _localizer["Roles.UpdatedSuccessfully"]
                );
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(_localizer["OperationCancelled"], request?.RoleId);
                return RequestResponse<ApplicationRole>.Fail(_localizer["OperationCancelled"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer["Roles.SystemError"], request?.RoleId);
                return RequestResponse<ApplicationRole>.InternalServerError(_localizer["Roles.SystemError"]);
            }
        }
        // حذف دور
        public async Task<RequestResponse<bool>> DeleteRoleAsync(
         string roleId,
         CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrEmpty(roleId))
                {
                    return RequestResponse<bool>.BadRequest(_localizer["Roles.InvalidRoleId"]);
                }

                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    _logger.LogWarning(_localizer["Roles.RoleNotFound"], roleId);
                    return RequestResponse<bool>.NotFound(_localizer["Roles.RoleNotFound"]);
                }

                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                if (usersInRole.Any())
                {
                    _logger.LogWarning(_localizer["Roles.RoleHasUsers"], role.Name);
                    return RequestResponse<bool>.BadRequest(_localizer["Roles.RoleHasUsers"]);
                }

                cancellationToken.ThrowIfCancellationRequested();

                var result = await _roleManager.DeleteAsync(role);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning(_localizer["Roles.DeleteRoleFailed"],
                        roleId, string.Join(", ", errors));
                    return RequestResponse<bool>.Fail("", errors);
                }


                _logger.LogInformation(_localizer["Roles.DeletedSuccessfully"], roleId);
                return RequestResponse<bool>.Ok(
                    data: true,
                    message: (_localizer["Roles.DeletedSuccessfully", role.Name])
                );
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(_localizer["OperationCancelled"], roleId);
                return RequestResponse<bool>.Fail(_localizer["OperationCancelled"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer["Roles.SystemError"], roleId);
                return RequestResponse<bool>.InternalServerError(_localizer["Roles.SystemError"]);
            }
        }


        public async Task<RequestResponse<bool>> RemoveUserFromRoleAsync(
          RemoveRoleRequest request,
          CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return RequestResponse<bool>.BadRequest(
                        errors: validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                }

                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning(_localizer["Roles.UserNotFound"], request.UserId);
                    return RequestResponse<bool>.NotFound(_localizer["Roles.UserNotFound"]);
                }

                var validRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync(cancellationToken);
                if (!validRoles.Contains(request.RoleName))
                {
                    _logger.LogWarning(_localizer["Roles.RoleNotFound"], request.RoleName);
                    return RequestResponse<bool>.NotFound(_localizer["Roles.RoleNotFound"]);
                }

                cancellationToken.ThrowIfCancellationRequested();

                if (!await _userManager.IsInRoleAsync(user, request.RoleName))
                {
                    _logger.LogWarning(_localizer["Roles.UserDoesNotHaveRole"], request.UserId, request.RoleName);
                    return RequestResponse<bool>.BadRequest(_localizer["Roles.UserDoesNotHaveRole"]);
                }

                var result = await _userManager.RemoveFromRoleAsync(user, request.RoleName);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning(_localizer["Roles.RemoveRoleFailed"],
                        request.RoleName, request.UserId, string.Join(", ", errors));
                    return RequestResponse<bool>.Fail("Fail", errors);
                }

                _logger.LogInformation(_localizer["Roles.RemovedSuccessfully"],
                    request.RoleName, request.UserId);
                return RequestResponse<bool>.Ok(
                    data: true,
                    message: _localizer["Roles.RemovedSuccessfully"]
                );
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(_localizer["OperationCancelled"], request?.UserId);
                return RequestResponse<bool>.Fail(_localizer["OperationCancelled"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer["Roles.SystemError"],
                    request?.RoleName, request?.UserId);
                return RequestResponse<bool>.InternalServerError(_localizer["Roles.SystemError"]);
            }
        }




        // الحصول على أدوار مستخدم
        public async Task<RequestResponse<IEnumerable<string>>> GetUserRolesAsync(
     string userId,
     CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning(_localizer["Validation.UserIdRequired"]);
                    return RequestResponse<IEnumerable<string>>.BadRequest(_localizer["Validation.UserIdRequired"]);
                }

                // Find the user
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning(_localizer["Roles.UserNotFound"], userId);
                    return RequestResponse<IEnumerable<string>>.NotFound(_localizer["Roles.UserNotFound"]);
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning(_localizer["Roles.UserInactive"], userId);
                    return RequestResponse<IEnumerable<string>>.BadRequest(_localizer["Roles.UserInactive"]);
                }

                cancellationToken.ThrowIfCancellationRequested();

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);

                _logger.LogInformation(_localizer["Roles.UserRolesRetrievedSuccessfully"], userId);
                return RequestResponse<IEnumerable<string>>.Ok(
                    data: roles,
                    message: _localizer["Roles.UserRolesRetrievedSuccessfully", userId]
                );
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(_localizer["Roles.OperationCancelled"]);
                return RequestResponse<IEnumerable<string>>.Fail(_localizer["Roles.OperationCancelled"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer["Roles.SystemError"]);
                return RequestResponse<IEnumerable<string>>.InternalServerError(_localizer["Roles.SystemError"]);
            }
        }
        // الحصول على دور بواسطة ID
        public async Task<RequestResponse<ApplicationRole>> GetRoleByIdAsync(
       string roleId,
       CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // التحقق من أن roleId ليس فارغاً
                if (string.IsNullOrEmpty(roleId))
                {
                    _logger.LogWarning("Empty role ID provided");
                    return RequestResponse<ApplicationRole>.BadRequest(_localizer["InvalidRoleId"]);
                }

                cancellationToken.ThrowIfCancellationRequested();

                // البحث عن الدور بواسطة المعرف
                var role = await _roleManager.FindByIdAsync(roleId);

                if (role == null)
                {
                    _logger.LogWarning("Role not found with ID: {RoleId}", roleId);
                    return RequestResponse<ApplicationRole>.NotFound(_localizer["RoleNotFound"]);
                }

                _logger.LogInformation("Role retrieved successfully with ID: {RoleId}", roleId);
                return RequestResponse<ApplicationRole>.Ok(
                    data: role,
                    message: _localizer["RoleRetrievedSuccessfully"]
                );
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("GetRoleById operation was cancelled for role ID: {RoleId}", roleId);
                return RequestResponse<ApplicationRole>.Fail("Operation was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving role with ID: {RoleId}", roleId);
                return RequestResponse<ApplicationRole>.InternalServerError(_localizer["SystemError"]);
            }
        }
        public async Task<RequestResponse<bool>> IsRoleExistsAsync(
    string roleName,
    CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(roleName))
                {
                    _logger.LogWarning(_localizer["Validation.RoleNameRequired"]);
                    return RequestResponse<bool>.BadRequest(
                        _localizer["Validation.RoleNameRequired"]);
                }
                var existingRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync(cancellationToken);

                var exists = existingRoles.Contains(roleName);

                _logger.LogInformation(_localizer["Roles.RoleExistenceChecked"], roleName, exists);
                return RequestResponse<bool>.Ok(
                    data: exists,
                    message: exists
                        ? _localizer["Roles.RoleExists", roleName]
                        : _localizer["Roles.RoleDoesNotExist", roleName]
                );
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(_localizer["OperationCancelled"]);
                return RequestResponse<bool>.Fail(_localizer["OperationCancelled"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer["Roles.SystemError"]);
                return RequestResponse<bool>.InternalServerError(_localizer["Roles.SystemError"]);
            }
        }
        public async Task<RequestResponse<bool>> IsInRoleAsync(
    string userId,
    string roleName,
    CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // التحقق من صحة الإدخالات
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roleName))
                {
                    _logger.LogWarning(_localizer["Validation.UserIdOrRoleNameRequired"]);
                    return RequestResponse<bool>.BadRequest(_localizer["Validation.UserIdOrRoleNameRequired"]);
                }

                // جلب المستخدم
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning(_localizer["Roles.UserNotFound"], userId);
                    return RequestResponse<bool>.NotFound(_localizer["Roles.UserNotFound"]);
                }

                // التحقق من الدور
                var isInRole = await _userManager.IsInRoleAsync(user, roleName);

                _logger.LogInformation(_localizer["Roles.CheckUserRoleSuccess"], userId, roleName, isInRole);
                return RequestResponse<bool>.Ok(
                    data: isInRole,
                    message: isInRole
                        ? _localizer["Roles.UserInRole", userId, roleName]
                        : _localizer["Roles.UserNotInRole", userId, roleName]
                );
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(_localizer["OperationCancelled"], userId);
                return RequestResponse<bool>.Fail(_localizer["OperationCancelled"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer["Roles.SystemError"], userId, roleName);
                return RequestResponse<bool>.InternalServerError(_localizer["Roles.SystemError"]);
            }
        }

        public async Task<RequestResponse<bool>> AddPermissionToRoleAsync(
          string roleId,
          int permissionId,
          CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    _logger.LogWarning(_localizer["Roles.RoleNotFound"], roleId);
                    return RequestResponse<bool>.NotFound(_localizer["Roles.RoleNotFound"]);
                }

                var exists = await _dbContext.RolePermissions
                    .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);
                if (exists)
                {
                    _logger.LogWarning(_localizer["Roles.PermissionAlreadyAssigned"], role.Name);
                    return RequestResponse<bool>.BadRequest(_localizer["Roles.PermissionAlreadyAssigned"]);
                }

                _dbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                });
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(_localizer["Roles.PermissionAddedSuccessfully"], role.Name);
                return RequestResponse<bool>.Ok(true, _localizer["Roles.PermissionAddedSuccessfully"]);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(_localizer["OperationCancelled"]);
                return RequestResponse<bool>.Fail(_localizer["OperationCancelled"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer["Roles.SystemError"]);
                return RequestResponse<bool>.InternalServerError(_localizer["Roles.SystemError"]);
            }
        }
        public async Task<RequestResponse<bool>> RemovePermissionFromRoleAsync(
             string roleId,
             int permissionId,
             CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var rolePermission = await _dbContext.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

                if (rolePermission == null)
                {
                    _logger.LogWarning(_localizer["Roles.PermissionNotFound"], roleId, permissionId);
                    return RequestResponse<bool>.NotFound(_localizer["Roles.PermissionNotFound"]);
                }

                _dbContext.RolePermissions.Remove(rolePermission);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(_localizer["Roles.PermissionRemovedSuccessfully"], roleId);
                return RequestResponse<bool>.Ok(true, _localizer["Roles.PermissionRemovedSuccessfully"]);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(_localizer["OperationCancelled"]);
                return RequestResponse<bool>.Fail(_localizer["OperationCancelled"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer["Roles.SystemError"]);
                return RequestResponse<bool>.InternalServerError(_localizer["Roles.SystemError"]);
            }
        }
        // تحديث Permission لدور (تغيير Permission موجود لآخر)
        public async Task<RequestResponse<bool>> UpdateRolePermissionAsync(
            string roleId,
            int oldPermissionId,
            int newPermissionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var rolePermission = await _dbContext.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == oldPermissionId, cancellationToken);

                if (rolePermission == null)
                {
                    _logger.LogWarning(_localizer["Roles.PermissionNotFound"], roleId, oldPermissionId);
                    return RequestResponse<bool>.NotFound(_localizer["Roles.PermissionNotFound"]);
                }

                var exists = await _dbContext.RolePermissions
                    .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == newPermissionId, cancellationToken);
                if (exists)
                {
                    _logger.LogWarning(_localizer["Roles.PermissionAlreadyAssigned"], roleId);
                    return RequestResponse<bool>.BadRequest(_localizer["Roles.PermissionAlreadyAssigned"]);
                }

                rolePermission.PermissionId = newPermissionId;
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(_localizer["Roles.PermissionUpdatedSuccessfully"], roleId);
                return RequestResponse<bool>.Ok(true, _localizer["Roles.PermissionUpdatedSuccessfully"]);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(_localizer["OperationCancelled"]);
                return RequestResponse<bool>.Fail(_localizer["OperationCancelled"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer["Roles.SystemError"]);
                return RequestResponse<bool>.InternalServerError(_localizer["Roles.SystemError"]);
            }
        }
        // جلب جميع Permissions لدور
        //public async Task<RequestResponse<List<Permission>>> GetPermissionsByRoleAsync(
        //    string roleId,
        //    CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        cancellationToken.ThrowIfCancellationRequested();

        //        var permissions = await _dbContext.RolePermissions
        //            .Where(rp => rp.RoleId == roleId)
        //            .Select(rp => rp.Permission)
        //            .ToListAsync(cancellationToken);

        //        return RequestResponse<List<Permission>>.Ok(permissions);
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        _logger.LogInformation(_localizer["OperationCancelled"]);
        //        return RequestResponse<List<Permission>>.Fail(_localizer["OperationCancelled"]);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, _localizer["Roles.SystemError"]);
        //        return RequestResponse<List<Permission>>.InternalServerError(_localizer["Roles.SystemError"]);
        //    }
        //}
        public async Task<RequestResponse<List<PermissionDTO>>> GetPermissionsByRoleAsync(
       string roleId,
       CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // التحقق من وجود الدور أولاً
                var roleExists = await _roleManager.Roles.AnyAsync(r => r.Id == roleId, cancellationToken);
                if (!roleExists)
                {
                    _logger.LogWarning(_localizer["Roles.RoleNotFound"], roleId);
                    return RequestResponse<List<PermissionDTO>>.NotFound(_localizer["Roles.RoleNotFound"]);
                }

                var permissions = await _dbContext.RolePermissions
                    .Where(rp => rp.RoleId == roleId)
                    .Join(
                        _dbContext.Permissions,
                        rp => rp.PermissionId,
                        p => p.Id,
                        (rp, p) => new PermissionDTO // ⬅️ استخدام DTO بدلاً من Entity
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Category = p.Category,
                            PermissionType = p.PermissionType,
                            Description = p.Description,
                        })
                    .ToListAsync(cancellationToken);

                return RequestResponse<List<PermissionDTO>>.Ok(permissions);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(_localizer["OperationCancelled"]);
                return RequestResponse<List<PermissionDTO>>.Fail(_localizer["OperationCancelled"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer["Roles.SystemError"]);
                return RequestResponse<List<PermissionDTO>>.InternalServerError(_localizer["Roles.SystemError"]);
            }
        }
        public async Task<RequestResponse<List<PermissionDTO>>> GetAllPermissionsAsync(
    CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var permissions = await _dbContext.Permissions
                    .Select(p => new PermissionDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Category = p.Category,
                        PermissionType = p.PermissionType,
                        Description = p.Description
                    })
                    .ToListAsync(cancellationToken);

                return RequestResponse<List<PermissionDTO>>.Ok(permissions);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(_localizer["OperationCancelled"]);
                return RequestResponse<List<PermissionDTO>>.Fail(_localizer["OperationCancelled"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer["Roles.SystemError"]);
                return RequestResponse<List<PermissionDTO>>.InternalServerError(_localizer["Roles.SystemError"]);
            }
        }
        public async Task<RequestResponse<bool>> UpdateUserRolesAsync(
    UpdateUserRolesRequest request,
    CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // التحقق من صحة الطلب
                var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return RequestResponse<bool>.BadRequest(
                        errors: validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                }

                // البحث عن المستخدم
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning(_localizer["Roles.UserNotFound"], request.UserId);
                    return RequestResponse<bool>.NotFound(_localizer["Roles.UserNotFound"]);
                }

                // التحقق من صحة الأدوار المقدمة
                var validRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync(cancellationToken);
                var invalidRoles = request.RoleNames.Except(validRoles).ToList();

                if (invalidRoles.Any())
                {
                    _logger.LogWarning(_localizer["Roles.InvalidRolesProvided"],
                        string.Join(", ", invalidRoles));
                    return RequestResponse<bool>.BadRequest(
                        _localizer["Roles.InvalidRolesProvided", string.Join(", ", invalidRoles)]
                    );
                }

                cancellationToken.ThrowIfCancellationRequested();

                // الحصول على أدوار المستخدم الحالية
                var currentRoles = await _userManager.GetRolesAsync(user);

                // حساب الأدوار التي يجب إضافتها وإزالتها
                var rolesToAdd = request.RoleNames.Except(currentRoles).ToList();
                var rolesToRemove = currentRoles.Except(request.RoleNames).ToList();

                // إذا لم يكن هناك تغيير
                if (!rolesToAdd.Any() && !rolesToRemove.Any())
                {
                    _logger.LogInformation(_localizer["Roles.NoChangesDetected"], request.UserId);
                    return RequestResponse<bool>.Ok(
                        data: true,
                        message: _localizer["Roles.NoChangesDetected"]
                    );
                }

                // استخدام transaction لضمان atomic operation
                using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    // إزالة الأدوار التي لم تعد مطلوبة
                    if (rolesToRemove.Any())
                    {
                        var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                        if (!removeResult.Succeeded)
                        {
                            var errors = removeResult.Errors.Select(e => e.Description).ToList();
                            _logger.LogWarning(_localizer["Roles.RemoveRolesFailed"],
                                request.UserId, string.Join(", ", rolesToRemove), string.Join(", ", errors));

                            await transaction.RollbackAsync(cancellationToken);
                            return RequestResponse<bool>.Fail(_localizer["Roles.RemoveRolesFailed"], errors);
                        }
                    }

                    // إضافة الأدوار الجديدة
                    if (rolesToAdd.Any())
                    {
                        var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                        if (!addResult.Succeeded)
                        {
                            var errors = addResult.Errors.Select(e => e.Description).ToList();
                            _logger.LogWarning(_localizer["Roles.AddRolesFailed"],
                                request.UserId, string.Join(", ", rolesToAdd), string.Join(", ", errors));

                            await transaction.RollbackAsync(cancellationToken);
                            return RequestResponse<bool>.Fail(_localizer["Roles.AddRolesFailed"], errors);
                        }
                    }

                    // حفظ التغييرات وإكمال العملية
                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation(_localizer["Roles.UserRolesUpdatedSuccessfully"],
                        request.UserId,
                        rolesToAdd.Count > 0 ? $"Added: {string.Join(", ", rolesToAdd)}" : "",
                        rolesToRemove.Count > 0 ? $"Removed: {string.Join(", ", rolesToRemove)}" : "");

                    return RequestResponse<bool>.Ok(
                        data: true,
                        message: _localizer["Roles.UserRolesUpdatedSuccessfully"]
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw; // سيتم التعامل معها في catch الخارجي
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(_localizer["OperationCancelled"], request?.UserId);
                return RequestResponse<bool>.Fail(_localizer["OperationCancelled"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer["Roles.UpdateUserRolesFailed"],
                    request?.UserId);
                return RequestResponse<bool>.InternalServerError(_localizer["Roles.SystemError"]);
            }
        }
    }
}
