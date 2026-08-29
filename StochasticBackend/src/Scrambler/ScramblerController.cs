using StochasticBackend.src.Auth.Attributes;
using StochasticBackend.src.Auth.Configuration;
using StochasticBackend.src.Scrambler.Services;
using StochasticBackend.src.Shared.Routing;

namespace StochasticBackend.src.Scrambler
{
    public class ScramblerController: IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/scramble");
            group.MapGet("/poison/{filename}", HandlePoisonRoute);
            group.MapGet("/poison/", HandlePoisonRoute);
        }

        [HasPermissions(EPermissionOperator.Or, UserPermissions.VIEW_IMAGES, UserPermissions.EDIT_IMAGES)]
        private static async Task<IResult> HandlePoisonRoute(HttpContext httpContext, IWebHostEnvironment env, string filename = "minthara-original.jpg")
        {
            string secureFolder = Path.Combine(env.ContentRootPath, "Images");
            string secureOriginsFolder = Path.Combine(secureFolder, "Originals");
            string filePath = Path.Combine(secureOriginsFolder, filename);

            string name = filename.Split('.')[0];
            string secureTargetFolder = Path.Combine(secureFolder, "Poisoned");
            string targetFilePath = Path.Combine(secureTargetFolder, $"{name}.gif");

            if (!File.Exists(filePath))
            {
                return Results.NotFound(); // 404 Safely Hidden
            }

            JitterRandomWaveBackgroundColorScrambler.PoisonImage(filePath, targetFilePath);

            // 3. Stream the file bytes securely to the authorized browser
            // This serves the GIF directly from memory without exposing a hard file path
            //return Results.File(filePath, "image/gif");
            return TypedResults.Ok("Image processed");
        }
    }
}
