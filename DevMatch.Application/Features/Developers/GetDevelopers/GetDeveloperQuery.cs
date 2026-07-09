using DevMatch.Application.Abstraction.Messaging;

namespace DevMatch.Application.Features.Developers.GetDevelopers
{ 
    public sealed record GetDeveloperQuery(
        Guid Id)
        : IQuery<GetDeveloperResponse>;
}
