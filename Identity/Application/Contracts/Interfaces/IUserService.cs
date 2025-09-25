using Common.Application.Common;
using Common.Contracts.DTO.Request.User;
using Identity.Application.Contracts.DTO.Common;
using Identity.Application.Contracts.DTO.Request.Users;
using Identity.Application.Contracts.DTO.Response.User;
using Identity.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Contracts.Interfaces
{
        public interface IUserService
        {

            /// <summary>
            /// تسجيل دخول المستخدم باستخدام اسم المستخدم أو البريد أو رقم الهاتف وكلمة المرور
            /// </summary>
            Task<RequestResponse<LoginDTO>> UserPasswordSignInAsync(LoginRequest request, CancellationToken cancellationToken = default);

            /// <summary>
            /// تسجيل دخول المستخدم (اختياري، نسخة مختلفة)
            /// </summary>
            Task<RequestResponse<LoginDTO>> SignInAsync(LoginRequest request, CancellationToken cancellationToken = default);

            /// <summary>
            /// تسجيل خروج المستخدم الحالي
            /// </summary>
            Task<RequestResponse> SignOutAsync(CancellationToken cancellationToken = default);

            /// <summary>
            /// تحديث توكن الدخول باستخدام RefreshToken
            /// </summary>
            Task<RequestResponse<LoginDTO>> RefreshTokenAsync(RefreshtokenRequest loginDTO, CancellationToken cancellationToken = default);

            /// <summary>
            /// تسجيل مستخدم جديد
            /// </summary>
            Task<RequestResponse<UserRegistrationResponse>> RegisterUser(RegisterRequest dto, CancellationToken cancellationToken = default);
        Task<RequestResponse> RemoveUserAsync(
          string userId,
          CancellationToken cancellationToken = default);
        Task<RequestResponse> SoftDeleteUserAsync(
string userId,
CancellationToken cancellationToken = default);
        Task<RequestResponse<PagedResult<UserResponse>>> GetAllUsersAsync(
UserQueryParameters queryParams,
CancellationToken cancellationToken = default);
        Task<RequestResponse<UserResponse>> UpdateUserAsync(
string userId,
UpdateUserRequest request,
CancellationToken cancellationToken = default);
        Task<RequestResponse> RestorUserAsync(
string userId,
CancellationToken cancellationToken = default);


    }
}
