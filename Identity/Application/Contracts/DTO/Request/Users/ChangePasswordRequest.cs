using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Contracts.DTO.Request.User
{
    public class ChangePasswordRequest
    {
        [JsonIgnore] // سيتم تعبئته من claim

        public string? UserId { get; set; }

        public string CurrentPassword { get; set; }


        public string NewPassword { get; set; }

        public string ConfirmNewPassword { get; set; }
    }
}
