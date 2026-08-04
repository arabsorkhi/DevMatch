using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Features.Recommendations.GenerateDailyRecommendations
{

    public sealed record Command(
        Guid DeveloperId,
        int Count = 5);
}
