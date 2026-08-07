using DevMatch.Application.Abstraction.Auth;
using DevMatch.Application.Abstraction.Authentication;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Domain.Entities.RecommendationFeedback;
using DevMatch.SharedKernel.Result;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Application.Features.Recommendations.Feedback;

public sealed class RecommendationFeedbackHandler
{
    private readonly IDevMatchDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;

    public RecommendationFeedbackHandler(
        IDevMatchDbContext dbContext,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<Result<RecommendationFeedbackResponse>> Handle(
        RecommendationFeedbackCommand command,
        CancellationToken cancellationToken)
    {
        Guid developerId = _currentUser.DeveloperId;
        bool issueIsAccessible = await _dbContext.GitIssues
            .AsNoTracking()
            .AnyAsync(
                issue =>
                    issue.Id == command.IssueId &&
                    (issue.GitRepository.DeveloperId == developerId ||
                     _dbContext.DailyRecommendations.Any(recommendation =>
                         recommendation.DeveloperId == developerId &&
                         recommendation.IssueId == issue.Id)),
                cancellationToken);

        if (!issueIsAccessible)
        {
            return Result<RecommendationFeedbackResponse>.Failure(
                Error.NotFound("Recommendations.IssueNotFound", "The selected recommendation was not found."));
        }

        DateTimeOffset now = _timeProvider.GetUtcNow();
        RecommendationFeedback? feedback = await _dbContext.RecommendationFeedback
            .SingleOrDefaultAsync(
                x => x.DeveloperId == developerId && x.IssueId == command.IssueId,
                cancellationToken);

        if (feedback is null)
        {
            feedback = RecommendationFeedback.Create(
                developerId,
                command.IssueId,
                command.Outcome,
                now);
            await _dbContext.RecommendationFeedback.AddAsync(feedback, cancellationToken);
        }
        else
        {
            feedback.ChangeOutcome(command.Outcome, now);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<RecommendationFeedbackResponse>.Success(
            new RecommendationFeedbackResponse(command.IssueId, command.Outcome, now));
    }
}
