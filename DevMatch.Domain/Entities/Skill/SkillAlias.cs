using DevMatch.SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.Entities.Skill
{
    //GitHub همیشه اسم Skill را یکسان نمی‌دهد
    //dotnet   یا  asp.net-core  یا  net8  همه یعنی  .NET
    //Need someone with ASP.NET Core and Redis experience

    //باید تبدیل شود به:

    //ASP.NET Core

    //Redis

    public sealed class SkillAlias : AuditableEntity<Guid>
    {
        private static readonly IReadOnlyDictionary<string, string> Canonical =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [NormalizeCore("dotnet")] = NormalizeCore(".NET"),
                [NormalizeCore(".net")] = NormalizeCore(".NET"),
                [NormalizeCore("net8")] = NormalizeCore(".NET"),
                [NormalizeCore("net9")] = NormalizeCore(".NET"),
                [NormalizeCore("aspnetcore")] = NormalizeCore("ASP.NET Core"),
                [NormalizeCore("asp.net-core")] = NormalizeCore("ASP.NET Core"),
                [NormalizeCore("asp net core")] = NormalizeCore("ASP.NET Core"),
                [NormalizeCore("efcore")] = NormalizeCore("Entity Framework Core"),
                [NormalizeCore("entityframeworkcore")] = NormalizeCore("Entity Framework Core"),
                [NormalizeCore("postgres")] = NormalizeCore("PostgreSQL"),
                [NormalizeCore("postgresql")] = NormalizeCore("PostgreSQL"),
                [NormalizeCore("rabbitmq")] = NormalizeCore("RabbitMQ")
            };
        private SkillAlias()
        {
        }

        public Guid SkillId { get; private set; }

        public string Alias { get; private set; } = null!;

        public Skill Skill { get; private set; } = null!;
        public static string Normalize(string? value)
        {
            string normalized = NormalizeCore(value);
            return Canonical.TryGetValue(normalized, out string? canonical)
                ? canonical
                : normalized;
        }

        public static bool AreEquivalent(string? left, string? right) =>
            string.Equals(Normalize(left), Normalize(right), StringComparison.Ordinal);

        private static string NormalizeCore(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var builder = new StringBuilder(value.Length);
            foreach (char ch in value.Trim().ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(ch) || ch is '#' or '+')
                    builder.Append(ch);
            }

            return builder.ToString();
        }
        public static SkillAlias Create(
            Guid skillId,
            string alias)
        {
            alias = alias.Trim();

            if (string.IsNullOrWhiteSpace(alias))
                throw new ArgumentException(nameof(alias));

            return new SkillAlias
            {
                Id = Guid.NewGuid(),

                SkillId = skillId,

                Alias = alias,

                CreatedAtUtc = DateTime.UtcNow
            };
        }

        public void Rename(string alias)
        {
            alias = alias.Trim();

            if (string.IsNullOrWhiteSpace(alias))
                throw new ArgumentException(nameof(alias));

            Alias = alias;

            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
