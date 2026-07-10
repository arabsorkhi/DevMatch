using DevMatch.SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.Entities.Developer
{
    //Domain نباید به EF وابسته شود.  no DataAnnotation
    // public sealed class Developer : AggregateRoot<Guid>
    //Developer
    // 
    // 1
    // 
    // ↓
    // 
    // ∞
    // 
    // Repository



    //Developer
    //     ↓
    // Developer Profile
    //     ↓
    // Skills
    //     ↓
    // Experience
    //     ↓
    // Interests
    //     ↓
    // Matching Engine
    //     ↓
    // Recommended Issues

    public sealed class Developer : AuditableEntity<Guid>
    {
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
        public bool IsDeleted { get; private set; }

        public DateTime? DeletedAtUtc { get; private set; }

        private readonly List<GitRepository.GitRepository> _repositories =
            new(); //no need to : developer.AddRepository(...)

        //ساخت Repository در Handler انجام می‌شود.
        public IReadOnlyCollection<GitRepository.GitRepository> Repositories
            => _repositories.AsReadOnly(); 

        public void Delete()
        {
            IsDeleted = true;

            DeletedAtUtc = DateTime.UtcNow;
        }

        public static Developer Create(
            string githubId,
            string userName,
            string? name,
            string? email,
            string? avatarUrl,
            string? bio,
            string? location)
        {
            return new Developer
            {
                Id = Guid.NewGuid(),

                GithubId = githubId,

                UserName = userName,

                Name = name,

                Email = email,

                AvatarUrl = avatarUrl,

                Bio = bio,

                Location = location,

                //  CreatedAtUtc = DateTime.UtcNow //Entity نباید بداند EF چه زمانی Save می‌کند.
            };
        }

        public void UpdateProfile(
            string? name,
            string? email,
            string? avatar,
            string? bio,
            string? location)
        {
            Name = name;

            Email = email;

            AvatarUrl = avatar;

            Bio = bio;

            Location = location;

         //   UpdatedAtUtc = DateTime.UtcNow;
        }

    }
}
