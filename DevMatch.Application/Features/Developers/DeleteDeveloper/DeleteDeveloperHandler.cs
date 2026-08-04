using DevMatch.Application.Abstraction.Messaging;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Application.Common.Error;
using DevMatch.Domain.Entities.Developer;
using DevMatch.SharedKernel.Result;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Application.Features.Developers.DeleteDeveloper
{
    public sealed class DeleteDeveloperHandler
        : ICommandHandler<
            DeleteDeveloperCommand,
            DeleteDeveloperResponse>
    {
        private readonly IDevMatchDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public DeleteDeveloperHandler(
            IDevMatchDbContext context,
            IUnitOfWork unitOfWork, TimeProvider timeProvider)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<Result<DeleteDeveloperResponse>> Handle(
            DeleteDeveloperCommand command,
            CancellationToken cancellationToken)
        {
            Developer? developer =
                await _context.Developers
                    .FirstOrDefaultAsync(
                        x => x.Id == command.Id,
                        cancellationToken);

            if (developer is null)
            {
                return Result<DeleteDeveloperResponse>.Failure(
                    DeveloperErrors.NotFound);
            }
            DateTimeOffset utcNow =
                _timeProvider.GetUtcNow();
            //_context.Developers.Remove(developer);
            developer.SoftDelete(utcNow);
            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return Result<DeleteDeveloperResponse>.Success(
                new DeleteDeveloperResponse(
                    developer.Id));
        }
    }
    }