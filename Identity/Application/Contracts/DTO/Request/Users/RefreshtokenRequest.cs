using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Contracts.DTO.Request.User
{
    public class RefreshtokenRequest
    {
        public string accessToken { get; set; }
        public string refreshToken { get; set; }

    }
}
