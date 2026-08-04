using DevMatch.Application.Abstraction;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Domain.Entities.DailyRecommendation;
using DevMatch.Domain.Entities.Developer;
using DevMatch.Domain.Entities.Matching;
using DevMatch.SharedKernel.Result;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Application.Features.Recommendations.GenerateDailyRecommendations
{


    //۱. Developer و اطلاعات مربوط به او از دیتابیس خوانده می‌شود.
    //۲. DeveloperMatchProfile ساخته می‌شود.
    //۳. Issueهای کاندید از دیتابیس خوانده می‌شوند.
    //۴. IssueMatchProfileها ساخته می‌شوند.
    //۵. MatchingService.RankIssues اجرا می‌شود.
    //۶. پنج نتیجه برتر به DailyRecommendation تبدیل می‌شوند.
    //۷. با IUnitOfWork ذخیره می‌شوند.



    public sealed class Handler
    {
        private const int CandidateMultiplier = 20;
        private const int MinimumCandidateCount = 100;

        private readonly IMatchingProfileReader _profileReader;
        private readonly IMatchingService _matchingService;
        private readonly IDevMatchDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public Handler(
            IMatchingProfileReader profileReader,
            IMatchingService matchingService,
            IDevMatchDbContext dbContext,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider)
        {
            _profileReader = profileReader;
            _matchingService = matchingService;
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<Result<Response>> Handle(
            Command command,
            CancellationToken cancellationToken)
        {
            DeveloperMatchProfile? developer =
                await _profileReader.GetDeveloperProfileAsync(
                    command.DeveloperId,
                    cancellationToken);

            if (developer is null)
            {
                return Result<Response>.Failure(
                    Errors.DeveloperNotFound(
                        command.DeveloperId));
            }

            int candidateLimit = Math.Max(
                command.Count * CandidateMultiplier,
                MinimumCandidateCount);

            IReadOnlyCollection<IssueMatchProfile> candidates =
                await _profileReader
                    .GetCandidateIssueProfilesAsync(
                        command.DeveloperId,
                        candidateLimit,
                        cancellationToken);

            if (candidates.Count == 0)
            {
                return Result<Response>.Failure(
                    Errors.NoCandidateIssues);
            }

            DateTimeOffset utcNow =
                _timeProvider.GetUtcNow();

            IReadOnlyList<MatchResult> matches =
                _matchingService.RankIssues(
                    developer,
                    candidates,
                    utcNow,
                    command.Count);

            DateTimeOffset startOfDayUtc =
                new(
                    utcNow.Year,
                    utcNow.Month,
                    utcNow.Day,
                    0,
                    0,
                    0,
                    TimeSpan.Zero);

            DateTimeOffset endOfDayUtc =
                startOfDayUtc.AddDays(1);

            var previousRecommendations =
                await _dbContext.DailyRecommendations
                    .Where(x =>
                        x.DeveloperId ==
                        command.DeveloperId &&
                        x.GeneratedAtUtc >=
                        startOfDayUtc &&
                        x.GeneratedAtUtc <
                        endOfDayUtc)
                    .ToListAsync(cancellationToken);

            if (previousRecommendations.Count > 0)
            {
                _dbContext.DailyRecommendations.RemoveRange(
                    previousRecommendations);
            }

            DailyRecommendation[] recommendations =
                matches
                    .Select((match, index) =>
                        DailyRecommendation.Create(
                            developerId:
                                command.DeveloperId,
                            issueId:
                                match.IssueId,
                            rank:
                                index + 1,
                            score:
                                match.Score,
                            generatedAtUtc:
                                utcNow))
                    .ToArray();

            if (recommendations.Length > 0)
            {
                await _dbContext.DailyRecommendations
                    .AddRangeAsync(
                        recommendations,
                        cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            RecommendationItem[] items =
                matches
                    .Select((match, index) =>
                        new RecommendationItem(
                            IssueId:
                                match.IssueId,
                            Rank:
                                index + 1,
                            Score:
                                match.Score,
                            ConfidenceMultiplier:
                                match.ConfidenceMultiplier,
                            VerificationMultiplier:
                                match.VerificationMultiplier,
                            Components:
                                match.Components,
                            MatchedSkills:
                                match.MatchedSkills,
                            MissingSkills:
                                match.MissingSkills,
                            Reasons:
                                match.Reasons))
                    .ToArray();

            var response = new Response(
                DeveloperId:
                    command.DeveloperId,
                GeneratedAtUtc:
                    utcNow,
                Recommendations:
                    items);

            return Result<Response>.Success(response);
        }
    }
}
  