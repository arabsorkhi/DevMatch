using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Common.Option
{
    public sealed class GitHubCatalogOptions
    {
        public const string SectionName = "GitHubCatalog";

        public string ApiBaseUrl { get; set; } = "https://api.github.com/";
        public string AccessToken { get; set; } = string.Empty;
        public string UserAgent { get; set; } = "DevMatch-ControlledCatalog/1.0";
        public int LowRateLimitThreshold { get; set; } = 100;
    }
}
