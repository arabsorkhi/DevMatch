using DevMatch.Api.Common.HttpResults;
using DevMatch.Application.Features.Recommendations.GenerateDailyRecommendations;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DevMatch.Api.Endpoints.Recommendations
{

//    محل اجرای خودکار روزانه

//    برای پیشنهاد روزانه دو ورودی می‌توانی داشته باشی:

//    API Endpoint
//    تولید دستی یا Refresh پیشنهادها

//    Worker / Quartz Job
//        تولید خودکار روزانه برای Developerهای فعال

//        Worker نباید مستقیماً IMatchingEngine را صدا بزند.باید همان GenerateDailyRecommendations.Handler را اجرا کند تا منطق ذخیره‌سازی و جلوگیری از تکرار در یک محل باقی بماند.


    public static class GenerateDailyRecommendationsEndpoint
    {
        public sealed record Request(int Count = 5);

        public static void MapEndpoint(
            IEndpointRouteBuilder app)
        {
            app.MapPost(
                "/developers/{developerId:guid}/recommendations/generate",
                HandleAsync);
        }

        private static async Task<
                Results<Ok<Response>, ProblemHttpResult>>
            HandleAsync(
                Guid developerId,
                Request request,
                Handler handler,
                CancellationToken cancellationToken)
        {
            var command = new Command(
                DeveloperId: developerId,
                Count: request.Count);

            var result = await handler.Handle(
                command,
                cancellationToken);

            return result.IsSuccess
                ? TypedResults.Ok(result.Value)
                : result.ToProblem();
        }
    }
}
