using DevMatch.Application.Abstraction;
using DevMatch.Application.Abstraction.Messaging;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Application.Common.Error;
using DevMatch.SharedKernel.Result;
using Microsoft.EntityFrameworkCore;


namespace DevMatch.Application.Features.Developers.CreateDevelopers
{
    //وابستگی به MediatR نداری. with Handler Pattern
    
    public sealed class CreateDeveloperHandler  : ICommandHandler< CreateDeveloperCommand, CreateDeveloperResponse>
    {
        private readonly IDevMatchDbContext _context;

        private readonly IUnitOfWork _unitOfWork;

        public CreateDeveloperHandler(
            IDevMatchDbContext context,
            IUnitOfWork unitOfWork)
        {
            _context = context;

            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CreateDeveloperResponse>>
            Handle(CreateDeveloperCommand command,CancellationToken cancellationToken)
        {
            var exists =
                await _context.Developers.AnyAsync(
                    x => x.GithubId == command.GithubId,
                    cancellationToken);

            if (exists)
            {
                return Result<CreateDeveloperResponse>.Failure(
                    DeveloperErrors.AlreadyExists);
            }

            var developer =
                Domain.Entities.Developer.Developer.Create(
                    command.GithubId,
                    command.UserName,
                    command.Name,
                    command.Email,
                    command.AvatarUrl,
                    command.Bio,
                    command.Location);

            _context.Developers.Add(developer);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return Result<CreateDeveloperResponse>.Success(
                new CreateDeveloperResponse(
                    developer.Id,
                    developer.UserName));
        }
    }


}
