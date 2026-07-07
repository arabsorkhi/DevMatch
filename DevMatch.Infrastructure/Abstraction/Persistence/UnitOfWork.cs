using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Application.Abstraction.Persistence;

namespace DevMatch.Infrastructure.Abstraction.Persistence
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly DevMatchDbContext _dbContext;

        public UnitOfWork(DevMatchDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
