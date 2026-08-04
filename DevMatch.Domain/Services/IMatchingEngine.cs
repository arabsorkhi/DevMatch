using DevMatch.Domain.Entities.Developer;
using DevMatch.Domain.Entities.Matching;

namespace DevMatch.Domain.Services
{
    public interface IMatchingEngine
    {
        //متدهای داخلی مانند CalculateActivityScore و BuildReasons جزئیات پیاده‌سازی Engine هستند.


        MatchResult Match(
            DeveloperMatchProfile developer,
            IssueMatchProfile issue,
            DateTimeOffset utcNow);

    }
  
}