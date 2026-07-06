using DevMatch.Domain.Entities.Developer;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Application.Persistence
{
    public interface IDevMatchDbContext
    {
        DbSet<Developer> Developers { get; }

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
