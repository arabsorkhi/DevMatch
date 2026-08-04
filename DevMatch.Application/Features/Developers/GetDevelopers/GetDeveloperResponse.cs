namespace DevMatch.Application.Features.Developers.GetDevelopers
{
    public sealed record GetDeveloperResponse(
        Guid Id,
        long GithubId,
        string UserName,
        string? Name,
        string? Email,
        string? AvatarUrl,
        string? Bio,
        string? Location);

}
