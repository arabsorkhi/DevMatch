using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Application.Abstraction.Messaging;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Application.Common.Error;
using DevMatch.SharedKernel.Result;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Application.Features.Developers.UpdateDevelopers
{
    public sealed class UpdateDeveloperHandler
        : ICommandHandler<
            UpdateDeveloperCommand,
            UpdateDeveloperResponse>
    {
        private readonly IDevMatchDbContext _context;

        private readonly IUnitOfWork _unitOfWork;

        public UpdateDeveloperHandler(
            IDevMatchDbContext context,
            IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<UpdateDeveloperResponse>> Handle(
            UpdateDeveloperCommand command,
            CancellationToken cancellationToken)
        {
            var developer =
                await _context.Developers
                    .FirstOrDefaultAsync(
                        x => x.Id == command.Id,
                        cancellationToken);

            if (developer is null)
            {
                return Result<UpdateDeveloperResponse>.Failure(
                    DeveloperErrors.NotFound);
            }

            developer.UpdateProfile(
                command.Name,
                command.Email,
                command.AvatarUrl,
                command.Bio,
                command.Location);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return Result<UpdateDeveloperResponse>.Success(
                new UpdateDeveloperResponse(
                    developer.Id));
        }
    }
}
