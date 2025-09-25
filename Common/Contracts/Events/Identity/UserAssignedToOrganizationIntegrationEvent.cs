using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Application.Contracts.Events.Identity
{
    public record UserAssignedToOrganizationIntegrationEvent(string UserId, Guid OrganizationId, string Role);

}
