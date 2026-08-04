using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Infrastructure.Authentication.Github
{
    //کلاس GitHubOptions بهتر است در Infrastructure باشد،
    //چون مربوط به نحوه پیکربندی یک Provider خارجی است
    public sealed class GitHubOptions
    {
        public const string SectionName = "GitHub";

        public string BaseUrl { get; init; }
            = "https://api.github.com";

        public string ClientId { get; init; } = string.Empty;

        public string ClientSecret { get; init; } = string.Empty;

        public string UserAgent { get; init; }
            = "DevMatch";

        public int TimeoutSeconds { get; init; } = 30;
        public string ApiVersion { get; init; } = "2026-03-10";
        public int PageSize { get; init; } = 100;
    }
}
