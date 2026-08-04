using DevMatch.Api.Common;
using DevMatch.Api.Infrastructure;
using DevMatch.Api.MiddleWares;
using DevMatch.Application;
using DevMatch.Application.Abstraction.Authentication;
using DevMatch.Application.Abstraction.Authentication.Github;
using DevMatch.Infrastructure.Authentication.Github;
using DevMatch.Infrastructure.DependancyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DevMatch.Infrastructure.Authentication.Jwt;

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
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

JwtOptions jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? new JwtOptions();

if (Encoding.UTF8.GetByteCount(jwtOptions.SigningKey) < 32)
{
    throw new InvalidOperationException(
        "Authentication:Jwt:SigningKey must contain at least 32 bytes. " +
        "Store it in environment variables or user-secrets, never in source control.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "github_username"
        };
    });



builder.Services
    .Configure<GitHubOptions>(builder.Configuration.GetSection(
        GitHubOptions.SectionName));

builder.Services
    .AddHttpClient<IGitHubClient, GitHubClient>(
        (provider, client) =>
        {
            var options =
                provider.GetRequiredService<
                    IOptions<GitHubOptions>>().Value;

            client.BaseAddress =
                new Uri(options.BaseUrl);

            client.Timeout =
                TimeSpan.FromSeconds(
                    options.TimeoutSeconds);

            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                options.UserAgent);

            client.DefaultRequestHeaders.Accept.ParseAdd(
                "application/vnd.github+json");
        });
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
app.UseGlobalExceptionHandling();
app.UseHttpsRedirection();
app.UseRequestLogging();
app.UseAuthorization();

app.MapControllers();
app.MapEndpoints();
app.Run();
