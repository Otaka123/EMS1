using Identity.Application.Contracts.DTO.Request.Roles;
using Identity.Application.Contracts.DTO.Response.Roles;
using Identity.Contracts.DTO.Request.Roles;
using Identity.Domain;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Contracts.Interfaces
{
    public interface IRoleService
    {
        Task<RequestResponse<bool>> UpdateUserRolesAsync(
    UpdateUserRolesRequest request,
    CancellationToken cancellationToken = default);
        Task<RequestResponse<IEnumerable<ApplicationRole>>> GetAllRolesAsync(CancellationToken cancellationToken = default);

        Task<RequestResponse<ApplicationRole>> AddNewRoleAsync(AddRoleRequest request, CancellationToken cancellationToken = default);

        Task<RequestResponse<ApplicationRole>> EditRoleAsync(EditRoleRequest request, CancellationToken cancellationToken = default);

        Task<RequestResponse<bool>> DeleteRoleAsync(string roleId, CancellationToken cancellationToken = default);

        Task<RequestResponse<bool>> AssignRoleToUserAsync(AssignRoleRequest request, CancellationToken cancellationToken = default);

        Task<RequestResponse<bool>> RemoveUserFromRoleAsync(RemoveRoleRequest request, CancellationToken cancellationToken = default);

        Task<RequestResponse<IEnumerable<string>>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default);

        Task<RequestResponse<ApplicationRole>> GetRoleByIdAsync(string roleId, CancellationToken cancellationToken = default);

        Task<RequestResponse<bool>> IsRoleExistsAsync(string roleName, CancellationToken cancellationToken = default);

        Task<RequestResponse<bool>> IsInRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default);

        Task<RequestResponse<bool>> AddPermissionToRoleAsync(string roleId, int permissionId, CancellationToken cancellationToken = default);

        Task<RequestResponse<bool>> RemovePermissionFromRoleAsync(string roleId, int permissionId, CancellationToken cancellationToken = default);

        Task<RequestResponse<bool>> UpdateRolePermissionAsync(string roleId, int oldPermissionId, int newPermissionId, CancellationToken cancellationToken = default);

        Task<RequestResponse<List<PermissionDTO>>> GetPermissionsByRoleAsync(
               string roleId,
               CancellationToken cancellationToken = default);
        Task<RequestResponse<List<PermissionDTO>>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
    }
}
