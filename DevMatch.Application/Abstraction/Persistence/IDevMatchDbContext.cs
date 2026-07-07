using DevMatch.Domain.Entities.Developer;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Application.Abstraction.Persistence
{
    public interface IDevMatchDbContext
    {
        DbSet<Developer> Developers { get; }

      
    }
}
