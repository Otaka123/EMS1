using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Exceptions
{
    public class TokenGenerationException : Exception
    {
        public TokenGenerationException()
        {
        }

        public TokenGenerationException(string message)
            : base(message)
        {
        }

        public TokenGenerationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        public string? UserId { get; init; }
        public Guid? OrganizationId { get; init; }

        public static TokenGenerationException ForUser(string userId, Exception innerException) =>
            new TokenGenerationException($"Failed to generate token for user {userId}", innerException)
            {
                UserId = userId
            };
    }
}
