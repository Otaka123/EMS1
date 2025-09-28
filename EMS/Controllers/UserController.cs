using Common.Application.Common;
using Common.Application.Contracts.interfaces;
using Common.Contracts.DTO.Request.User;
using Identity.Application.Contracts.DTO.Common;
using Identity.Application.Contracts.DTO.Request.Roles;
using Identity.Application.Contracts.DTO.Request.Users;
using Identity.Application.Contracts.DTO.Response.Roles;
using Identity.Application.Contracts.DTO.Response.User;
using Identity.Application.Contracts.Enum;
using Identity.Application.Contracts.Interfaces;
using Identity.Infrastructure.Services.Roles;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        private readonly ISharedMessageLocalizer _localizer;
        private readonly IRoleService _roleService;
        public UserController(IUserService userService, ILogger<UserController> logger, ISharedMessageLocalizer localizer)
        {
            _userService = userService;
            _logger = logger;
            _localizer = localizer;
        }
        /// <summary>
        /// User login endpoint
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Authentication result with JWT token</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(RequestResponse<LoginDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status423Locked)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]

        public async Task<ActionResult<RequestResponse<LoginDTO>>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _userService.SignInAsync(request, cancellationToken);

                return result.IsSuccess switch
                {
                    true => Ok(result),
                    false when result.ErrorR == ResponseError.Unauthorized => Unauthorized(result),
                    false when result.ErrorR == ResponseError.BadRequest => BadRequest(result),
                    false when result.ErrorR == ResponseError.UnprocessableEntity => UnprocessableEntity(result),
                    false when result.ErrorR == ResponseError.Locked => StatusCode(423, result),
                    false when result.ErrorR == ResponseError.NotFound => NotFound(result),
                    _ => StatusCode(StatusCodes.Status500InternalServerError,
                        RequestResponse.InternalServerError("An unexpected error occurred"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for : {Email}", request.UserNameOrEmailOrPhone);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError("An unexpected error occurred during login"));
            }
        }
        /// <summary>
        /// User sign-out endpoint
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result indicating whether sign-out was successful</returns>
        [HttpPost("signout")]
        [Authorize]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RequestResponse>> SignOut(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _userService.SignOutAsync(cancellationToken);

                return result.IsSuccess switch
                {
                    true => Ok(result),
                    false when result.ErrorR == ResponseError.Unauthorized => Unauthorized(result),
                    false when result.ErrorR == ResponseError.BadRequest => BadRequest(result),
                    false when result.ErrorR == ResponseError.NotFound => NotFound(result),
                    _ => StatusCode(StatusCodes.Status500InternalServerError,
                        RequestResponse.InternalServerError("An unexpected error occurred"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sign-out error for current user");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError("An unexpected error occurred during sign-out"));
            }
        }
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshtokenRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.RefreshTokenAsync(request, cancellationToken);

            if (!result.IsSuccess)
                return Unauthorized(result);

            return Ok(result);
        }
        ///// <summary>
        ///// Register new user
        ///// </summary>
        ///// <param name="dto">User registration Extensions</param>
        ///// <returns>Registration result with user ID</returns>
        //[HttpPost("register")]
        //[ProducesResponseType(typeof(RequestResponse<UserRegistrationResponse>), StatusCodes.Status201Created)]
        //[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status409Conflict)]
        //[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status422UnprocessableEntity)]
        //[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<RequestResponse<UserRegistrationResponse>>> Register([FromBody] RegisterRequest dto, CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        var result = await _userService.RegisterUser(dto, cancellationToken);

        //        return result.IsSuccess switch
        //        {
        //            true => Ok(

        //                result),
        //            false when result.ErrorR == ResponseError.BadRequest && result.Message == "EmailAlreadyExists" =>
        //                Conflict(result),
        //            false when result.ErrorR == ResponseError.BadRequest =>
        //                BadRequest(result),
        //            false when result.ErrorR == ResponseError.UnprocessableEntity =>
        //                UnprocessableEntity(result),
        //            _ => StatusCode(StatusCodes.Status500InternalServerError,
        //                RequestResponse.InternalServerError("An unexpected error occurred during registration"))
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Registration error for email: {Email}", dto.Email);
        //        return StatusCode(StatusCodes.Status500InternalServerError,
        //            RequestResponse.InternalServerError("An unexpected error occurred during registration"));
        //    }
        //}
        /// <summary>
        /// Get all users with pagination and filtering
        /// </summary>
        /// <param name="searchTerm">Search term for filtering users</param>
        /// <param name="isActive">Filter by active status</param>
        /// <param name="gender">Filter by gender</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of users</returns>
        [HttpGet("All")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RequestResponse<PagedResult<UserResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<PagedResult<UserResponse>>>> GetUsers(
            [FromQuery] string searchTerm = "",
            [FromQuery] bool? isActive = null,
            [FromQuery] string gender = "",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var parameters = new UserQueryParameters
                {
                    SearchTerm = searchTerm,
                    IsActive = isActive,
                    Gender = !string.IsNullOrEmpty(gender) ?
                           (GenderType)Enum.Parse(typeof(GenderType), gender) : null,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _userService.GetAllUsersAsync(parameters, cancellationToken);

                return result.IsSuccess switch
                {
                    true => Ok(result),
                    false when result.ErrorR == ResponseError.BadRequest => BadRequest(result),
                    false when result.ErrorR == ResponseError.NotFound => NotFound(result),
                    _ => StatusCode(StatusCodes.Status500InternalServerError,
                        RequestResponse.InternalServerError(_localizer["LoadUsersError"]))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching users");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["LoadUsersError"]));
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User details</returns>
        [HttpGet("{id}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RequestResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<UserResponse>>> GetUserById(
            [FromRoute] string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(RequestResponse.BadRequest(_localizer["UserIdRequired"]));
                }

                var result = await _userService.GetUserByIdAsync(id, cancellationToken);

                return result.IsSuccess switch
                {
                    true => Ok(result),
                    false when result.ErrorR == ResponseError.NotFound => NotFound(result),
                    false when result.ErrorR == ResponseError.BadRequest => BadRequest(result),
                    _ => StatusCode(StatusCodes.Status500InternalServerError,
                        RequestResponse.InternalServerError(_localizer["LoadUserError"]))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching user with ID: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["LoadUserError"]));
            }
        }

        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="request">User registration data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created user result</returns>
        [HttpPost("Create")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RequestResponse<UserRegistrationResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<UserRegistrationResponse>>> CreateUser(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _userService.RegisterUser(request, cancellationToken);

                return result.IsSuccess switch
                {
                    true => Ok(result),
                    false when result.ErrorR == ResponseError.BadRequest && result.Message == "EmailAlreadyExists" =>
                        Conflict(result),
                    false when result.ErrorR == ResponseError.BadRequest => BadRequest(result),
                    false when result.ErrorR == ResponseError.UnprocessableEntity => UnprocessableEntity(result),
                    _ => StatusCode(StatusCodes.Status500InternalServerError,
                        RequestResponse.InternalServerError(_localizer["CreateUserError"]))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["CreateUserError"]));
            }
        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="request">User update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Update result</returns>
        [HttpPut("{id}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse>> UpdateUser(
            [FromRoute] string id,
            [FromBody] UpdateUserRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(RequestResponse.BadRequest(_localizer["UserIdRequired"]));
                }

                var result = await _userService.UpdateUserAsync(id, request, cancellationToken);

                return result.IsSuccess switch
                {
                    true => Ok(result),
                    false when result.ErrorR == ResponseError.NotFound => NotFound(result),
                    false when result.ErrorR == ResponseError.BadRequest => BadRequest(result),
                    false when result.ErrorR == ResponseError.UnprocessableEntity => UnprocessableEntity(result),
                    _ => StatusCode(StatusCodes.Status500InternalServerError,
                        RequestResponse.InternalServerError(_localizer["UpdateUserError"]))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user with ID: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["UpdateUserError"]));
            }
        }

    

        /// <summary>
        /// Soft delete user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Delete result</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse>> DeleteUser(
            [FromRoute] string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(RequestResponse.BadRequest(_localizer["UserIdRequired"]));
                }

                var result = await _userService.RemoveUserAsync(id, cancellationToken);

                return result.IsSuccess switch
                {
                    true => Ok(result),
                    false when result.ErrorR == ResponseError.NotFound => NotFound(result),
                    false when result.ErrorR == ResponseError.BadRequest => BadRequest(result),
                    _ => StatusCode(StatusCodes.Status500InternalServerError,
                        RequestResponse.InternalServerError(_localizer["DeleteUserError"]))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user with ID: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["DeleteUserError"]));
            }
        }

        /// <summary>
        /// Soft delete user (disable)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Soft delete result</returns>
        [HttpPatch("{id}/soft-delete")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse>> SoftDeleteUser(
            [FromRoute] string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(RequestResponse.BadRequest(_localizer["UserIdRequired"]));
                }

                var result = await _userService.SoftDeleteUserAsync(id, cancellationToken);

                return result.IsSuccess switch
                {
                    true => Ok(result),
                    false when result.ErrorR == ResponseError.NotFound => NotFound(result),
                    false when result.ErrorR == ResponseError.BadRequest => BadRequest(result),
                    _ => StatusCode(StatusCodes.Status500InternalServerError,
                        RequestResponse.InternalServerError(_localizer["SoftDeleteUserError"]))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while soft deleting user with ID: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["SoftDeleteUserError"]));
            }
        }

        /// <summary>
        /// Restore soft deleted user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Restore result</returns>
        [HttpPatch("{id}/restore")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse>> RestoreUser(
            [FromRoute] string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(RequestResponse.BadRequest(_localizer["UserIdRequired"]));
                }

                var result = await _userService.RestorUserAsync(id, cancellationToken);

                return result.IsSuccess switch
                {
                    true => Ok(result),
                    false when result.ErrorR == ResponseError.NotFound => NotFound(result),
                    false when result.ErrorR == ResponseError.BadRequest => BadRequest(result),
                    _ => StatusCode(StatusCodes.Status500InternalServerError,
                        RequestResponse.InternalServerError(_localizer["RestoreUserError"]))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while restoring user with ID: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["RestoreUserError"]));
            }
        }

        /// <summary>
        /// Check user status
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User status information</returns>
        [HttpGet("{id}/status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RequestResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<object>>> CheckUserStatus([FromRoute] string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(RequestResponse.BadRequest(_localizer["UserIdRequired"]));
                }

                var result = await _userService.GetUserByIdAsync(id);

                if (!result.IsSuccess)
                {
                    return NotFound(result);
                }

                var statusResponse = new
                {
                    isActive = result.Data.IsActive,
                    lastLogin = result.Data.LastLoginDate
                };

                return Ok(RequestResponse<object>.Success(statusResponse, "User status retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking user status with ID: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["CheckUserStatusError"]));
            }
        }
    }
}

