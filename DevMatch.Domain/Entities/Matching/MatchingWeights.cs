using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.Entities.Matching
{

    public sealed record MatchingWeights
    {
        public decimal Skill { get; init; } = 0.35m;

        public decimal Repository { get; init; } = 0.10m;

        public decimal Contribution { get; init; } = 0.10m;

        public decimal Activity { get; init; } = 0.10m;

        public decimal Preference { get; init; } = 0.15m;

        public decimal History { get; init; } = 0.10m;

        public decimal Level { get; init; } = 0.10m;

        public decimal Total =>
            Skill +
            Repository +
            Contribution +
            Activity +
            Preference +
            History +
            Level;


     
            public void Validate()
            {
                if (Total != 1m)
                {
                    throw new InvalidOperationException(
                        $"Matching weights must total 1. Current total: {Total}.");
                }

                if (Skill < 0m ||
                    Repository < 0m ||
                    Contribution < 0m ||
                    Activity < 0m ||
                    Preference < 0m ||
                    History < 0m ||
                    Level < 0m)
                {
                    throw new InvalidOperationException(
                        "Matching weights cannot be negative.");
                }
            }
        }


    //public sealed record MatchingWeights(
    //    decimal Skill,
    //    decimal Repository,
    //    decimal Contribution,
    //    decimal Activity,
    //    decimal Preference,
    //    decimal History,
    //    decimal Level)
    //{
    //    public static MatchingWeights Default { get; } = new(
    //        Skill: 0.35m,
    //        Repository: 0.10m,
    //        Contribution: 0.10m,
    //        Activity: 0.10m,
    //        Preference: 0.15m,
    //        History: 0.10m,
    //        Level: 0.10m);

    //    public decimal Total =>
    //        Skill +
    //        Repository +
    //        Contribution +
    //        Activity +
    //        Preference +
    //        History +
    //        Level;

    //    public void EnsureValid()
    //    {
    //        decimal[] values =
    //        [
    //            Skill,
    //            Repository,
    //            Contribution,
    //            Activity,
    //            Preference,
    //            History,
    //            Level
    //        ];

    //        if (values.Any(value => value is < 0m or > 1m))
    //        {
    //            throw new InvalidOperationException(
    //                "Matching weights must be between 0 and 1.");
    //        }

    //        if (Math.Abs(Total - 1m) > 0.0001m)
    //        {
    //            throw new InvalidOperationException(
    //                $"Matching weights must total 1. Current total: {Total}.");
    //        }
    //    }
   // }
}