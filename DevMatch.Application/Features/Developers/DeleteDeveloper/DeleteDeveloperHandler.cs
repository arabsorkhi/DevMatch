using DevMatch.Application.Abstraction.Messaging;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Application.Common.Error;
using DevMatch.SharedKernel.Result;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Application.Features.Developers.DeleteDeveloper
{

    public sealed class DeleteDeveloperHandler

        : ICommandHandler<DeleteDeveloperCommand, DeleteDeveloperResponse>
    {
        private readonly IDevMatchDbContext _context;

        private readonly IUnitOfWork _unitOfWork;

        public DeleteDeveloperHandler(

            IDevMatchDbContext context,IUnitOfWork unitOfWork)
        {
            _context = context;

            _unitOfWork = unitOfWork;
        }

        public async Task<Result<DeleteDeveloperResponse>> Handle(
            DeleteDeveloperCommand command,
            CancellationToken cancellationToken)
        {
            var developer = await _context.Developers
                .FirstOrDefaultAsync(
                    x => x.Id == command.Id,
                    cancellationToken);

            if (developer is null)
            {
                return Result<DeleteDeveloperResponse>.Failure(
                    DeveloperErrors.NotFound);
            }

            developer.Delete();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<DeleteDeveloperResponse>.Success(
                new DeleteDeveloperResponse(developer.Id));
        }
    }
}