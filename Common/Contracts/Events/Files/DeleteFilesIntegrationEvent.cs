using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Application.Contracts.Events.Files
{
    public sealed record DeleteFilesIntegrationEvent(
        IReadOnlyCollection<Guid> FileIds,string DeleteByUserId
    );

}
