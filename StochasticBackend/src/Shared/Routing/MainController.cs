using Microsoft.AspNetCore.Antiforgery;
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
            group.MapGet("/", HandleMainRoute);
            group.MapGet("/antiforgery/token", HandleAntiforgeryToken);
        }

        [HasPermissions(EPermissionOperator.Or, UserPermissions.VIEW_IMAGES, UserPermissions.EDIT_IMAGES)]
        private static async Task<IResult> HandleMainRoute()
        {
            return TypedResults.Ok("hi");
        }

        [Authorize]
        private static async Task<IResult> HandleAntiforgeryToken(IAntiforgery antiforgery, HttpContext httpContext)
        {
            AntiforgeryTokenSet? tokens = antiforgery.GetAndStoreTokens(httpContext);
            httpContext.Response.Cookies.Append(
                "XSRF-TOKEN", 
                tokens.RequestToken!, 
                new CookieOptions 
                { 
                    HttpOnly = false, 
                    Secure = true, 
                    SameSite = SameSiteMode.Lax 
                }
            );

            return TypedResults.Ok();
        }
    }
}
