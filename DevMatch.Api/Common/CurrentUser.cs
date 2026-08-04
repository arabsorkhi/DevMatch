using DevMatch.Application.Abstraction.Auth;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace DevMatch.Api.Common
{
    public sealed class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?
                .User.Identity?
                .IsAuthenticated == true;

        public Guid DeveloperId
        {
            get
            {
                string? subject = _httpContextAccessor
                    .HttpContext?
                    .User
                    .FindFirstValue(JwtRegisteredClaimNames.Sub);

                return Guid.TryParse(subject, out Guid id)
                    ? id
                    : throw new UnauthorizedAccessException();
            }
        }
    }
}
