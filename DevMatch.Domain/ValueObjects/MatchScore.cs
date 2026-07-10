using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.ValueObjects
{
    //یعنی MatchResult یک Value Object است که MatchScore هم داخل آن قرار دارد.

    //کاربر می‌خواهد بداند:

    //چرا 89٪؟
    //کدام Skillها Match شدند؟
    //کدام Skillها کم هستند؟
    //چه چیزی را یاد بگیرد؟

    //پس خروجی نباید فقط یک عدد باشد.
    public sealed record MatchScore
    {
        public int Percentage { get; }

        public MatchScore(int percentage)
        {
            Percentage = Math.Clamp(
                percentage,
                0,
                100);
        }

        public bool IsQualified
            => Percentage >= 70;

        public override string ToString()
            => $"{Percentage}%";
    }
}
