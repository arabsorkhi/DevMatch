using DevMatch.Api.Infrastructure;
using DevMatch.Api.MiddleWares;
using DevMatch.Application;
using DevMatch.Infrastructure.DependancyInjection;

var builder = WebApplication.CreateBuilder(args);
//each error got it :{
//   "title": "Developer.NotFound",
//   "status": 404,
//   "traceId": "...",
//   "timestamp": "2026-07-09T17:45:10Z"
// }
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier;

        context.ProblemDetails.Extensions["timestamp"] =
            DateTime.UtcNow;
    };
});

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddEndpoints(typeof(Program).Assembly); 
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseGlobalExceptionHandling();
app.MapControllers();
app.MapEndpoints();
app.Run();
