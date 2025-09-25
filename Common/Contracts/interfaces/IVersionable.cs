using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Application.Contracts.interfaces
{

    public interface IVersionable
    {
        byte[] RowVersion { get; set; } // للتحكم في التزامن

    }
}
