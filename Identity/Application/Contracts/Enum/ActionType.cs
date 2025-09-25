using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Contracts.Enum
{
    public enum ActionType : byte // tinyint
     { Created = 1, Updated = 2, Deleted = 3 , read = 4}
}
