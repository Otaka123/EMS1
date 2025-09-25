using Identity.API.Areas.Admin.ViewModel;
using Identity.Application.Contracts.DTO.Response.Roles;
using Identity.Application.Contracts.Interfaces;
using Identity.Contracts.DTO.Request.Roles;
using Identity.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly IRoleService _roleService;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RolesController(IRoleService roleService, RoleManager<ApplicationRole> roleManager, UserManager<AppUser> userManager)
        {
            this._roleService = roleService;
            this._roleManager = roleManager;
            _userManager = userManager;
        }

        // GET: Admin/Roles
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            var result = await _roleService.GetAllRolesAsync(cancellationToken);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return View(new List<IdentityRole>());
            }

            return View(result.Data);
        }

        // GET: Admin/Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddRoleRequest request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var result = await _roleService.AddNewRoleAsync(request, cancellationToken);

            if (result.IsSuccess)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(request);
        }

        // GET: Admin/Roles/Edit/5
        public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var result = await _roleService.GetRoleByIdAsync(id, cancellationToken);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            var editRequest = new EditRoleRequest
            {
                RoleId = result.Data.Id,
                NewRoleName = result.Data.Name
            };

            return View(editRequest);
        }

        // POST: Admin/Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditRoleRequest request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var result = await _roleService.EditRoleAsync(request, cancellationToken);

            if (result.IsSuccess)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Permissions(string id)
        {
            try
            {
                // التحقق من وجود الدور
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    TempData["ErrorMessage"] = "الدور غير موجود";
                    return RedirectToAction("Index");
                }

                // جلب الصلاحيات الحالية للدور - مع معالجة الحالة الفارغة
                List<PermissionDTO> currentPermissions = new List<PermissionDTO>();
                var currentPermissionsResult = await _roleService.GetPermissionsByRoleAsync(id);

                if (currentPermissionsResult.IsSuccess && currentPermissionsResult.Data != null)
                {
                    currentPermissions = currentPermissionsResult.Data;
                }
                else if (!currentPermissionsResult.IsSuccess)
                {
                    // إذا فشل جلب الصلاحيات، نعتبر أن لا توجد صلاحيات (بدون خطأ)
                    currentPermissions = new List<PermissionDTO>();
                }

                // جلب جميع الصلاحيات المتاحة - مع معالجة الحالة الفارغة
                List<PermissionDTO> allPermissions = new List<PermissionDTO>();
                var allPermissionsResult = await _roleService.GetAllPermissionsAsync();

                if (allPermissionsResult.IsSuccess && allPermissionsResult.Data != null)
                {
                    allPermissions = allPermissionsResult.Data;
                }
                else
                {
                    // إذا لم توجد صلاحيات، نستخدم قائمة فارغة
                    allPermissions = new List<PermissionDTO>();
                }

                var viewModel = new RolePermissionsViewModel
                {
                    RoleId = id,
                    RoleName = role.Name,
                    CurrentPermissions = currentPermissions,
                    AllPermissions = allPermissions
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل الصلاحيات";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Permissions(RolePermissionsViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // إعادة تعبئة النموذج في حالة الخطأ
                    model.AllPermissions =  _roleService.GetAllPermissionsAsync().Result.Data;
                    return View(model);
                }

                // جلب الصلاحيات الحالية للدور
                var currentPermissionsResult = await _roleService.GetPermissionsByRoleAsync(model.RoleId);
                var currentPermissionIds = currentPermissionsResult.Data.Select(p => p.Id).ToList();

                // تحديد الصلاحيات المراد إضافتها
                var permissionsToAdd = model.SelectedPermissionIds.Except(currentPermissionIds).ToList();

                // تحديد الصلاحيات المراد إزالتها
                var permissionsToRemove = currentPermissionIds.Except(model.SelectedPermissionIds).ToList();

                // إضافة الصلاحيات الجديدة
                foreach (var permissionId in permissionsToAdd)
                {
                    var result = await _roleService.AddPermissionToRoleAsync(model.RoleId, permissionId);
                    if (!result.IsSuccess)
                    {
                        // التعامل مع الخطأ
                        ModelState.AddModelError("", $"فشل إضافة الصلاحية: {permissionId}");
                    }
                }

                // إزالة الصلاحيات غير المحددة
                foreach (var permissionId in permissionsToRemove)
                {
                    var result = await _roleService.RemovePermissionFromRoleAsync(model.RoleId, permissionId);
                    if (!result.IsSuccess)
                    {
                        // التعامل مع الخطأ
                        ModelState.AddModelError("", $"فشل إزالة الصلاحية: {permissionId}");
                    }
                }

                if (ModelState.ErrorCount == 0)
                {
                    TempData["SuccessMessage"] = "تم تحديث صلاحيات الدور بنجاح";
                    return RedirectToAction("Permissions", new { roleId = model.RoleId });
                }

                // إعادة تعبئة النموذج في حالة وجود أخطاء
                model.AllPermissions =  _roleService.GetAllPermissionsAsync().Result.Data;
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "حدث خطأ أثناء تحديث الصلاحيات");
                return View(model);
            }
        }
        // POST: Admin/Roles/RemovePermissionAjax (للاستخدام مع AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RemovePermissionAjax(string roleId, int permissionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(roleId) || permissionId <= 0)
            {
                return Json(new { success = false, message = "بيانات غير صالحة" });
            }

            var result = await _roleService.RemovePermissionFromRoleAsync(roleId, permissionId, cancellationToken);

            if (result.IsSuccess)
            {
                return Json(new { success = true, message = result.Message });
            }
            else
            {
                return Json(new { success = false, message = result.Message });
            }
        }
        // POST: Admin/Roles/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var result = await _roleService.DeleteRoleAsync(id, cancellationToken);

            if (result.IsSuccess)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }
        // في ملف RolesController.cs - إضافة هذه الـ Actions

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "معرف المستخدم غير صالح";
                    return RedirectToAction("UsersList", "Users");
                }

                // جلب بيانات المستخدم
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "المستخدم غير موجود";
                    return RedirectToAction("UsersList", "Users");
                }

                // جلب جميع الأدوار
                var allRoles = await _roleManager.Roles.ToListAsync();

                // جلب أدوار المستخدم الحالية
                var userRoles = await _userManager.GetRolesAsync(user);

                var viewModel = new UserRolesViewModel
                {
                    UserId = userId,
                    UserName = user.UserName,
                    Email = user.Email,
                    UserCurrentRoles = userRoles.ToList(),
                    AllRoles = allRoles.Select(role => new UserRoleInfo
                    {
                        RoleId = role.Id,
                        RoleName = role.Name,
                        IsSelected = userRoles.Contains(role.Name)
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل الصفحة";
                return RedirectToAction("UsersList", "Users");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "بيانات غير صالحة";
                    return RedirectToAction("UsersList", "Users");
                }

                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "المستخدم غير موجود";
                    return RedirectToAction("UsersList", "Users");
                }

                // جلب أدوار المستخدم الحالية
                var currentUserRoles = await _userManager.GetRolesAsync(user);

                // جلب جميع الأدوار المحددة
                var selectedRoles = new List<string>();
                foreach (var roleId in request.SelectedRoleIds)
                {
                    var role = await _roleManager.FindByIdAsync(roleId);
                    if (role != null)
                    {
                        selectedRoles.Add(role.Name);
                    }
                }

                // تحديد الأدوار المراد إضافتها
                var rolesToAdd = selectedRoles.Except(currentUserRoles).ToList();

                // تحديد الأدوار المراد إزالتها
                var rolesToRemove = currentUserRoles.Except(selectedRoles).ToList();

                // إضافة الأدوار الجديدة
                if (rolesToAdd.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                    if (!addResult.Succeeded)
                    {
                        var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                        TempData["ErrorMessage"] = $"فشل إضافة الأدوار: {errors}";
                        return RedirectToAction("ManageUserRoles", new { userId = request.UserId });
                    }
                }

                // إزالة الأدوار غير المحددة
                if (rolesToRemove.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    if (!removeResult.Succeeded)
                    {
                        var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                        TempData["ErrorMessage"] = $"فشل إزالة الأدوار: {errors}";
                        return RedirectToAction("ManageUserRoles", new { userId = request.UserId });
                    }
                }

                TempData["SuccessMessage"] = "تم تحديث أدوار المستخدم بنجاح";
                return RedirectToAction("ManageUserRoles", new { userId = request.UserId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحديث الأدوار";
                return RedirectToAction("ManageUserRoles", new { userId = request.UserId });
            }
        }
    }
}
