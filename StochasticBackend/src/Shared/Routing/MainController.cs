using Microsoft.AspNetCore.Authorization;
using StochasticBackend.src.Auth.Attributes;
using StochasticBackend.src.Auth.Configuration;
using StochasticBackend.src.Auth.Extensions;

namespace StochasticBackend.src.Shared.Routing
{
    public class MainController : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/");
            group.MapGet("/", HandleMainRoute)
                .RequirePermissions(EPermissionOperator.Or, "users:delete", "admin:all"); ;
        }

        [HasPermissions(EPermissionOperator.And, "images:view", "images:edit")]
        private static async Task<IResult> HandleMainRoute()
        {
            return TypedResults.Ok("hi");
        }
    }
}
