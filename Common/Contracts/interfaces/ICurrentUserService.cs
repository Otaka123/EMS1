using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Application.Contracts.interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? Email { get; }
        bool IsInRole(string role);
        List<string> Roles { get; }
        Guid? OrganizationId { get; }
        bool IsAuthenticated { get; }
    }
}
