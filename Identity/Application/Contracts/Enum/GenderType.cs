using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Contracts.Enum
{
    public enum GenderType : byte
    {
        Unknown = 0,
        Male = 1,
        Female = 2,
        Other = 3,
        PreferNotToSay = 4
    }
}
