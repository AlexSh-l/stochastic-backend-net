namespace stochastic_backend_net.src.Shared.Routing
{
    public interface IEndpoint
    {
        abstract static void MapEndpoint(IEndpointRouteBuilder builder);
    }
}
