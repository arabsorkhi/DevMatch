using DevMatch.SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.Entities.Developer
{
    //Domain نباید به EF وابسته شود.  no DataAnnotation
    public sealed class Developer : AggregateRoot<Guid>
    {
        // dont use :new Developer()
        private Developer()
        {
        }

        public string GithubId { get; private set; } = null!;

        public string UserName { get; private set; } = null!;

        public string? Name { get; private set; }

        public string? Email { get; private set; }

        public string? AvatarUrl { get; private set; }

        public string? Bio { get; private set; }

        public string? Location { get; private set; }

        public static Developer Create(
            string githubId,
            string userName)
        {
            return new Developer
            {
                Id = Guid.NewGuid(),
                GithubId = githubId,
                UserName = userName,
                CreatedAtUtc = DateTime.UtcNow
            };
        }
    }
}
