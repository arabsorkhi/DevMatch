using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.Entities.Matching.Score
{
    internal static class ScoreClamp
    {
        public static decimal Value(decimal value) => Math.Clamp(value, 0m, 100m);
    }

    public readonly record struct RepositoryScore
    {
        public RepositoryScore(decimal value) => Value = ScoreClamp.Value(value);
        public decimal Value { get; }
        public static implicit operator decimal(RepositoryScore score) => score.Value;
    }

    public readonly record struct ContributionScore
    {
        public ContributionScore(decimal value) => Value = ScoreClamp.Value(value);
        public decimal Value { get; }
        public static implicit operator decimal(ContributionScore score) => score.Value;
    }

    public readonly record struct ActivityScore
    {
        public ActivityScore(decimal value) => Value = ScoreClamp.Value(value);
        public decimal Value { get; }
        public static implicit operator decimal(ActivityScore score) => score.Value;
    }

    public readonly record struct PreferenceScore
    {
        public PreferenceScore(decimal value) => Value = ScoreClamp.Value(value);
        public decimal Value { get; }
        public static implicit operator decimal(PreferenceScore score) => score.Value;
    }

    public readonly record struct HistoryScore
    {
        public HistoryScore(decimal value) => Value = ScoreClamp.Value(value);
        public decimal Value { get; }
        public static implicit operator decimal(HistoryScore score) => score.Value;
    }

}
