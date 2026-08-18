namespace StochasticBackend.src.Shared.Routing
{
    public interface IEndpoint
    {
        abstract static void MapEndpoint(IEndpointRouteBuilder builder);
    }
}
