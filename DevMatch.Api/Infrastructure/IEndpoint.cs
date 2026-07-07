namespace DevMatch.Api.Infrastructure
{
    //هر Feature , Endpoint خودش را دارد.
    public interface IEndpoint
    {
        void MapEndpoint(IEndpointRouteBuilder app);
    }
}
