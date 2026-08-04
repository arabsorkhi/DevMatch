using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.SharedKernel.Result;

namespace DevMatch.Application.Features.Recommendations.GenerateDailyRecommendations
{
    public static class Errors
    {
        public static Error DeveloperNotFound(Guid developerId) =>
            Error.NotFound(
                "Recommendations.DeveloperNotFound",
                $"Developer '{developerId}' was not found.");

        public static readonly Error NoCandidateIssues =
            Error.NotFound(
                "Recommendations.NoCandidateIssues",
                "No candidate issues were found.");
    }
}
