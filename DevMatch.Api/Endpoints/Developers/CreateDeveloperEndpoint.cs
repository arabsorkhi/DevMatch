using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Api.Common.HttpResults;
using DevMatch.Api.Filters;
using DevMatch.Api.Infrastructure;
using DevMatch.Application.Features.Developers.CreateDevelopers;

namespace DevMatch.Api.Endpoints.Developers
{
    //نه Controller.     نه Service.    نه Repository.
    //Endpoint    می‌داند:
    //HTTP
    // 
    // Route
    // 
    // StatusCode
    // 
    // Headers
    //endpoint : چون Endpoint باید به Abstraction وابسته باشد، نه کلاس Concrete.
    //الان Endpoint مستقیماً Handler را می‌شناسد.
    public sealed class CreateDeveloperEndpoint : IEndpoint
    {
        public void MapEndpoint(
            IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "/developers", //از Named Endpoint استفاده می‌کنیم.
                    Handle)
                .AddEndpointFilter <
                ValidationFilter<CreateDeveloperCommand>>();//چون Validation باید قبل از Handler اجرا شود.
        }

        private static async Task<IResult> Handle(

            CreateDeveloperCommand command,

            CreateDeveloperHandler handler,

            CancellationToken cancellationToken)
        {
            var result =
                await handler.Handle(
                    command,
                    cancellationToken);

            //return result.ToCreatedResult(
            //    "GetDeveloper",
            //    new
            //    {
            //        id = result.Value!.Id
            //    });

            if (result.IsFailure)
                return result.ToProblemResult();

            return TypedResults.CreatedAtRoute(

                result.Value,

                "GetDeveloper",

                new
                {
                    id = result.Value!.Id
                });
        }


        //async (
        //    CreateDeveloperCommand command,
        //    CreateDeveloperHandler handler, //الان Endpoint مستقیماً Handler را می‌شناسد.
        //    CancellationToken cancellationToken) =>
        //{
        //    var result =
        //        await handler.Handle(
        //            command,
        //            cancellationToken);

        //    //ProblemDetails استاندارد ASP.NET Core
        //    //if (result.IsFailure)
        //    //    return Results.Problem(

        //    //        title: result.Error.Code,

        //    //        detail: result.Error.Description,

        //    //        statusCode: result.Error.StatusCode);

        //    if (result.IsFailure)
        //        return result.ToProblemResult(); //is in result extension

        //    //return Results.Created(
        //    //    $"/developers/{result.Value!.Id}",
        //    //    result.Value);
        //    //return result.ToCreatedResult(
        //    //    $"/developers/{result.Value?.Id}"); //چون Endpoint دارد از Response خبر 


        //    //   return result.ToCreatedResult(result.Value!.Location);
        //    if (result.Value != null)
        //        return TypedResults.CreatedAtRoute(
        //            result.Value,
        //            "GetDeveloper", //اسم آن Endpoint
        //            new
        //            {
        //                id = result.Value.Id
        //            });
        //})
        //.AddEndpointFilter<
        //    ValidationFilter<CreateDeveloperCommand>>();
    }
    }
 
