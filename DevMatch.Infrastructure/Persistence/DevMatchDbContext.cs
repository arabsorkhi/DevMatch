using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Application.Persistence;
using DevMatch.Domain.Entities.Developer;
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

namespace DevMatch.Infrastructure.Persistence
{
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
    }
}
