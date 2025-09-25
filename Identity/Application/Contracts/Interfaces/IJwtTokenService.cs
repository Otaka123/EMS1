using Identity.Application.Contracts.DTO.Response.User;
using Identity.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Contracts.Interfaces
{
    public interface IJwtTokenService
    {
        //Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(string idToken);
        Task<string> GenerateJwtToken(
   AppUser user,
   List<string>? roles = null,
   List<Permission>? userPermissions = null,
   CancellationToken cancellationToken = default);
        Task<string> VerifyFacebookToken(string accessToken);
        Task<string> GenerateJwtToken(AppUser user, List<string>? roles = null);
        Task<string> GenerateJwtToken(
    AppUser user,
    List<string>? roles = null,
    CancellationToken cancellationToken = default);
        Task<LoginDTO> GenerateJwtWithRefreshTokenAsync(
      AppUser user,
      List<string>? roles = null,
      CancellationToken cancellationToken = default);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        Task<LoginDTO> GenerateJwtWithRefreshTokenAsync(
AppUser user,
CancellationToken cancellationToken = default);

        //        Task<string> GenerateJwtTokenWithOrganizationId(
        //         AppUser user,
        //         CancellationToken cancellationToken = default);
        //        Task<LoginDTO> GenerateJwt_OrgWithRefreshTokenAsync(
        //AppUser user,
        //CancellationToken cancellationToken = default);
        Task<bool> RevokeRefreshTokenAsync(
    string userId,
    CancellationToken cancellationToken = default);

    }
}

