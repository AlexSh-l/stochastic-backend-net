namespace stochastic_backend_net.src.Shared.Routing
{
    public class MainController : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/");
            group.MapGet("/", HandleMainRoute);
        }

        private static Task<IResult> HandleMainRoute()
        {
            return Task.FromResult(Results.Ok("hi"));
        }
    }
}
