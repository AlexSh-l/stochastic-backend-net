using stochastic_backend_net.src.Shared.Routing;

namespace stochastic_backend_net.src.Auth
{
    public class AuthController : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/auth");
            group.MapGet("/login", HandleLogin);
            group.MapGet("/register", HandleRegister);
        }

        private static Task<IResult> HandleLogin()
        {
            return Task.FromResult(Results.Json(new { message = "user is logged in" }));
        }

        private static Task<IResult> HandleRegister()
        {
            return Task.FromResult(Results.Ok("user just signed in"));
        }
    }
}
