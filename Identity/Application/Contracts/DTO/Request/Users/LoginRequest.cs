using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Contracts.DTO.Request.User
{
    public class LoginRequest
    {

        public string UserNameOrEmailOrPhone { get; set; }
        public string Password { get; set; }
        public bool IsPersistent { get; set; }

    }
}
