using Common.Application.Common;
using Common.Application.Contracts.interfaces;
using Identity.Application.Contracts.DTO.Request.Roles;
using Identity.Application.Contracts.DTO.Response.Roles;
using Identity.Application.Contracts.Interfaces;
using Identity.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<PermissionController> _logger;
        private readonly ISharedMessageLocalizer _localizer;

        public PermissionController(
            IRoleService roleService,
            ILogger<PermissionController> logger,
            ISharedMessageLocalizer localizer)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        /// <summary>
        /// Add permission to role
        /// </summary>
        [HttpPost("{roleId}/AddPermissionToRole/{permissionId}")]
        [ProducesResponseType(typeof(RequestResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<bool>>> AddPermissionToRole(
            string roleId,
            int permissionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _roleService.AddPermissionToRoleAsync(roleId, permissionId, cancellationToken);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding permission {PermissionId} to role {RoleId}", permissionId, roleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["SystemError"]));
            }
        }

        /// <summary>
        /// Remove permission from role
        /// </summary>
        [HttpDelete("{roleId}/RemovePermissionFromRole/{permissionId}")]
        [ProducesResponseType(typeof(RequestResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<bool>>> RemovePermissionFromRole(
            string roleId,
            int permissionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _roleService.RemovePermissionFromRoleAsync(roleId, permissionId, cancellationToken);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing permission {PermissionId} from role {RoleId}", permissionId, roleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["SystemError"]));
            }
        }

        /// <summary>
        /// Update role permission (replace old permission with new one)
        /// </summary>
        [HttpPut("{roleId}/UpdateRolePermission")]
        [ProducesResponseType(typeof(RequestResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<bool>>> UpdateRolePermission(
            string roleId,
            [FromBody] UpdateRolePermissionRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _roleService.UpdateRolePermissionAsync(
                    roleId,
                    request.OldPermissionId,
                    request.NewPermissionId,
                    cancellationToken);

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating permission for role {RoleId}", roleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["SystemError"]));
            }
        }

        /// <summary>
        /// Get all permissions for a role
        /// </summary>
        [HttpGet("{roleId}/GetRolePermissions")]
        [ProducesResponseType(typeof(RequestResponse<List<Permission>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<List<PermissionDTO>>>> GetRolePermissions(
            string roleId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _roleService.GetPermissionsByRoleAsync(roleId, cancellationToken);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions for role {RoleId}", roleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["SystemError"]));
            }
        }
        protected ActionResult<RequestResponse<T>> HandleResult<T>(RequestResponse<T> response)
        {
            if (response.IsSuccess)
            {
                return Ok(response); // إرجاع النتيجة مغلفة في RequestResponse
            }

            return response.ErrorR switch
            {
                ResponseError error => error.StatusCode switch
                {
                    400 => BadRequest(response),
                    401 => Unauthorized(response),
                    403 => Forbid(), // أو StatusCode(403, response)
                    404 => NotFound(response),
                    409 => Conflict(response),
                    422 => UnprocessableEntity(response),
                    _ => StatusCode(error.StatusCode, response)
                },
                _ => StatusCode(StatusCodes.Status500InternalServerError, response)
            };
        }

    }
}
