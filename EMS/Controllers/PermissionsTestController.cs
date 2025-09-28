using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionsTestController : ControllerBase
    {
        private readonly ILogger<PermissionsTestController> _logger;

        public PermissionsTestController(ILogger<PermissionsTestController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// Test User CRUD permissions
        /// </summary>
        [HttpGet("user/create")]
        [Authorize(Policy = "UserManagement:Write:Users.Create")]
        [ProducesResponseType(typeof(RequestResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<RequestResponse<string>> TestCreateUserPermission()
        {
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation(($"{claim.Type} = {claim.Value}"));
            }
            return RequestResponse<string>.Ok("User has CreateUser permission");
        }

    }
}
