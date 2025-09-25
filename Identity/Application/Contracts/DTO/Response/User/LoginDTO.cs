using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Contracts.DTO.Response.User
{
    public class LoginDTO
    {
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiresAt { get; }
        public DateTime RefreshTokenExpiresAt { get; }

        public string RefreshToken { get; set; }
        public LoginDTO(string JWTtoken, string RefreshToken)
        {
            this.AccessToken = JWTtoken;
            this.RefreshToken = RefreshToken;
            this.AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(30);
            this.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        }
    }
}
