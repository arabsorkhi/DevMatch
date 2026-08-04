using DevMatch.Application.Abstraction.Messaging;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Domain.Entities.Developer;
using DevMatch.SharedKernel.Result;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Application.Features.Developers.UpdateDevelopers
{
    //نام SynchronizeDeveloperHandler منطقی‌تر است چون اطلاعات Developer را با پروفایل GitHub همگام می‌کنی.
    public sealed class UpsertDeveloperHandler
        : ICommandHandler<
            UpdateDeveloperCommand,
            UpdateDeveloperResponse>
    {
        private readonly IDevMatchDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public UpsertDeveloperHandler(
            IDevMatchDbContext context,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<Result<UpdateDeveloperResponse>> Handle(
            UpdateDeveloperCommand request,
            CancellationToken cancellationToken)
        {
            DateTimeOffset utcNow = _timeProvider.GetUtcNow();

            Developer? developer =
                await _context.Developers
                    .FirstOrDefaultAsync(
                        x => x.Id == request.Id,
                        cancellationToken);

            if (developer is null)
            {
                developer = Developer.Create(
                    request.GitHubUserId,
                    request.GitHubUsername,
                    request.DisplayName,
                    request.Email,
                    request.AvatarUrl,
                    request.Bio,
                    request.Location);

                _context.Developers.Add(developer);
            }

            developer.SynchronizeGitHubProfile(
                request.GitHubUserId,
                request.GitHubUsername,
                request.DisplayName,
                request.Email,
                request.AvatarUrl,
                request.Bio,
                request.Location,
                request.Company,
                request.BlogUrl,
                utcNow);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return Result<UpdateDeveloperResponse>.Success(
                new UpdateDeveloperResponse(
                    developer.Id));
        }
    }
}
