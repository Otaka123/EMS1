using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Application.Contracts.interfaces
{
    public interface IAuditable
    {
        string CreatedBy { get; set; }
        DateTime CreatedAt { get; set; }
        string? ModifiedBy { get; set; }
        DateTime? ModifiedAt { get; set; }
        void TrackCreation(string by);
        void TrackModification(string by);
    }

}
