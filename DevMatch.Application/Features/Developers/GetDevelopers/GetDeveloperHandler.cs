using DevMatch.Application.Abstraction.Messaging;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Application.Common.Error;
using DevMatch.SharedKernel.Result;
using Microsoft.EntityFrameworkCore;


namespace DevMatch.Application.Features.Developers.GetDevelopers
{
    //وابستگی به MediatR نداری. with Handler Pattern
    //CreateDeveloper
    // 
    // ↓
    // 
    // Developer ساخته شد
    // 
    // ↓
    // 
    // Response


    public sealed class GetDeveloperHandler
        : IQueryHandler<GetDeveloperQuery, GetDeveloperResponse>
    {
        private readonly IDevMatchDbContext _context;

        public GetDeveloperHandler(
            IDevMatchDbContext context)
        {
            _context = context;
        }

        public async Task<Result<GetDeveloperResponse>> Handle(
            GetDeveloperQuery query,
            CancellationToken cancellationToken)
        {
            var developer =
                await _context.Developers
                    .AsNoTracking()//Query هیچ چیزی را تغییر نمی‌دهد.EF دیگر Entity را Track نمی‌کند.
                    .FirstOrDefaultAsync(
                        x => x.Id == query.Id,
                        cancellationToken);

            if (developer is null)
            {
                return Result<GetDeveloperResponse>.Failure(
                    DeveloperErrors.NotFound);
            }

            return Result<GetDeveloperResponse>.Success(
                new GetDeveloperResponse(
                    developer.Id,
                    developer.GithubId,
                    developer.UserName,
                    developer.Name,
                    developer.Email,
                    developer.AvatarUrl,
                    developer.Bio,
                    developer.Location));
        }


    }
}
