using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StochasticBackend.src.Auth.Attributes;
using StochasticBackend.src.Auth.Configuration;
using StochasticBackend.src.Scrambler.Configuration;
using StochasticBackend.src.Scrambler.DTO;
using StochasticBackend.src.Scrambler.Exceptions;
using StochasticBackend.src.Scrambler.Services;
using StochasticBackend.src.Shared.Routing;

namespace StochasticBackend.src.Scrambler
{
    public class ScramblerController: IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/scramble");
            group.MapPost("/poison", HandlePoisonRoute);
        }

        [HasPermissions(EPermissionOperator.Or, UserPermissions.VIEW_IMAGES, UserPermissions.EDIT_IMAGES)]
        [RequireAntiforgeryToken]
        private static async Task<IResult> HandlePoisonRoute([FromForm] ScrambleFileRequestDTO requestDTO, HttpContext httpContext, IWebHostEnvironment env, IScramblerService scramblerService)
        {
            if (httpContext.User?.Identity?.IsAuthenticated != true) return TypedResults.Unauthorized();

            var file = requestDTO.File;
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest("No file uploaded.");
            }

            var filterName = requestDTO.FilterName;
            EScramblerTypes filter = EScramblerTypes.ScramblerRegular;
            if (!string.IsNullOrEmpty(filterName))
            {
                Enum.TryParse<EScramblerTypes>(filterName, true, out filter);
            }

            string secureFolder = Path.Combine(env.ContentRootPath, "Images");
            string secureOriginsFolder = Path.Combine(secureFolder, "Originals");
            string filePath = Path.Combine(secureOriginsFolder, file.FileName);

            if (!Directory.Exists(secureOriginsFolder))
            {
                Directory.CreateDirectory(secureOriginsFolder);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string name = Guid.NewGuid().ToString();
            string secureTargetFolder = Path.Combine(secureFolder, "Poisoned");
            string targetFilePath = Path.Combine(secureTargetFolder, $"{name}.gif");

            try
            {
                await scramblerService.PoisonImageAsync(filter, filePath, targetFilePath);
                return Results.File(targetFilePath, "image/gif");
            }
            catch (ScramblerException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Server Is Busy"
                );
            }
            catch (Exception ex)
            {
                return Results.Problem(
                   detail: "An unexpected error occurred while processing the image.",
                   statusCode: StatusCodes.Status500InternalServerError,
                   title: "Error"
               );
            }
        }
    }
}
