namespace DevMatch.Application.Common.Error
{
    //Application نباید هیچ اطلاعی از HTTP داشته باشد.
    //Validation 400
    //Unauthorized 401
    //Forbidden 403

    public static class DeveloperErrors
    {
        public static readonly SharedKernel.Result.Error AlreadyExists =
            new(
                "Developer.AlreadyExists",
                "Developer already exists."
          //     , StatusCodes.Status409Conflict
                );//یعنی لایه Application به ASP.NET Core وابسته شده است.این خلاف Clean Architecture است.

        public static readonly SharedKernel.Result.Error NotFound =
            new(
                "Developer.NotFound",
                "Developer was not found."
              //  ,StatusCodes.Status404NotFound
              );
    }
}
