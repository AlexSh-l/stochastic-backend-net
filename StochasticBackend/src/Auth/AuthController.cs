using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using StochasticBackend.src.Auth.DTO;
using StochasticBackend.src.Auth.Entities;
using StochasticBackend.src.Auth.Services;
using StochasticBackend.src.Shared.DatabasePSQL;
using StochasticBackend.src.Shared.Routing;
using System.Security.Claims;

namespace StochasticBackend.src.Auth
{
    public class AuthController : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/auth").WithTags("Auth");
            group.MapPost("/login", HandleLogin);
            group.MapPost("/register", HandleRegister);
            group.MapGet("/logout", HandleLogout);
        }

        private static async Task HandleLogin(LoginFormDTO loginForm, HttpContext context, ApplicationContext dbContext, IAuthService authService)
        {
            if (string.IsNullOrEmpty(loginForm.Login) || string.IsNullOrEmpty(loginForm.Password))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { message = "Login or Password are empty" });
                return;
            }

            string login = loginForm.Login;
            string password = loginForm.Password;

            UserDTO? user = await authService.LogUserInAsync(login, password);
            if (user is null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Unable to find user" });
                return;
            }

            var claims = new List<Claim> { 
                new(ClaimTypes.Name, user.Id.ToString()),
                new(ClaimTypes.Role, user.Role.Name)
            };
            ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal claimsPrincipal = new(claimsIdentity);

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
            await context.Response.WriteAsJsonAsync(new { message = "user is logged in" });
        }

        private static async Task<IResult> HandleRegister(LoginFormDTO loginForm, HttpContext context, ApplicationContext dbContext, IAuthService authService)
        {
            if (string.IsNullOrEmpty(loginForm.Login) || string.IsNullOrEmpty(loginForm.Password))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { message = "Login or Password are empty" });
                return Results.BadRequest();
            }

            string login = loginForm.Login;
            string password = loginForm.Password;

            UserDTO? user = await authService.RegisterUserAsync(login, password);
            if (user is null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Unable to register user" });
                return Results.BadRequest();
            }

            var claims = new List<Claim> {
                new(ClaimTypes.Name, user.Id.ToString()),
                new(ClaimTypes.Role, user.Role.Name)
            };
            ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal claimsPrincipal = new(claimsIdentity);

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            return TypedResults.Ok("user just signed in");
        }

        private static async Task HandleLogout(HttpContext context)
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await context.Response.WriteAsJsonAsync(new { message = "user successfully logged out" });
        }
    }
}
