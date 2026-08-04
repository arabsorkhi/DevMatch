using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using DevMatch.Application.Abstraction.Authentication;

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
        public long? GitHubUserId =>
            long.TryParse(Principal?.FindFirstValue("github_id"), out long id)
                ? id
                : null;

        public string? GitHubUsername =>
            Principal?.FindFirstValue("github_username");

        public string? Email =>
            Principal?.FindFirstValue("email");

        private ClaimsPrincipal? Principal =>
            _httpContextAccessor.HttpContext?.User;
    }
}
