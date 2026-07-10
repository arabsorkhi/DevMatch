using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.SharedKernel.Common;

namespace DevMatch.Domain.Entities.Skill
{
    public sealed class Skill
        : AuditableEntity<Guid>
    {
        private Skill()
        {
        }

        public string Name { get; private set; } = null!;
       
        //Redis یا  redis یا REDIS
        public string NormalizedName { get; private set; } = null!;

        public string? Description { get; private set; }

        public bool IsActive { get; private set; }

        public static Skill Create(
            string name,
            string? description)
        {
            return new Skill
            {
                Id = Guid.NewGuid(),

                Name = name.Trim(),

                NormalizedName =
                    name.Trim().ToUpperInvariant(),

                Description = description,

                IsActive = true,

                CreatedAtUtc = DateTime.UtcNow
            };
        }

        public void Disable()
        {
            IsActive = false;

            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Enable()
        {
            IsActive = true;

            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
