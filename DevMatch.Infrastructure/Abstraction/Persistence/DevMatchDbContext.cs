using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Domain.Entities.Developer;
using DevMatch.SharedKernel.Common;
using Microsoft.EntityFrameworkCore;

//Clean Architecture

//         Api
//    /           \
//   /             \
// Application   Infrastructure
//    \            /
//     \          /
//        Domain


//Infrastructure

//    ↓

//Application

//    ↓

//Domain

namespace DevMatch.Infrastructure.Abstraction.Persistence
{
    //حالا DbContext مسئول Audit است.

    public class DevMatchDbContext
        : DbContext, IDevMatchDbContext
    {
        public DevMatchDbContext(
            DbContextOptions<DevMatchDbContext> options)
            : base(options)
        {
        }

        public DbSet<Developer> Developers => Set<Developer>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(DevMatchDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            UpdateAuditableEntities();

            return await base.SaveChangesAsync(
                cancellationToken);
        }

        private void UpdateAuditableEntities()
        {
            var entries =
                ChangeTracker
                    .Entries<AuditableEntity<Guid>>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAtUtc = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
                }
            }
        }
    }
}
