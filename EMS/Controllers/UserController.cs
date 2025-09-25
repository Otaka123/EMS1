using Common.Application.Common;
using Common.Application.Contracts.interfaces;
using Common.Contracts.DTO.Request.User;
using Identity.Application.Contracts.DTO.Response.User;
using Identity.Application.Contracts.Interfaces;
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
        /// <summary>
        /// Register new user
        /// </summary>
        /// <param name="dto">User registration Extensions</param>
        /// <returns>Registration result with user ID</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RequestResponse<UserRegistrationResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<UserRegistrationResponse>>> Register([FromBody] RegisterRequest dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _userService.RegisterUser(dto, cancellationToken);

                return result.IsSuccess switch
                {
                    true => Ok(

                        result),
                    false when result.ErrorR == ResponseError.BadRequest && result.Message == "EmailAlreadyExists" =>
                        Conflict(result),
                    false when result.ErrorR == ResponseError.BadRequest =>
                        BadRequest(result),
                    false when result.ErrorR == ResponseError.UnprocessableEntity =>
                        UnprocessableEntity(result),
                    _ => StatusCode(StatusCodes.Status500InternalServerError,
                        RequestResponse.InternalServerError("An unexpected error occurred during registration"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error for email: {Email}", dto.Email);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError("An unexpected error occurred during registration"));
            }
        }

    }
}
