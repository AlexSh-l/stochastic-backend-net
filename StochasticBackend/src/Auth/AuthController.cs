using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using StochasticBackend.src.Auth.Entities;
using StochasticBackend.src.Shared.Routing;
using System.Security.Claims;

namespace StochasticBackend.src.Auth
{
    public class AuthController : IEndpoint
    {
        private static List<User> users = new List<User> {
            new User { Login = "TestUser1", Password = "TestUser12345", Role = new Role() },
            new User { Login = "TestUser2", Password = "TestUser23456", Role = new Role() }
        };

        public static void MapEndpoint(IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/auth");
            group.MapGet("/login", HandleLogin);
            group.MapGet("/register", HandleRegister);
            group.MapGet("/logout", HandleLogout);
        }

        private static async void HandleLogin(HttpContext context)
        {
            string login = "TestUser1";
            User? user = users.FirstOrDefault(u => u.Login == login);
            if (user is null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "unable to find user" });
                return;
            }

            var claims = new List<Claim> { 
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };
            ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal claimsPrincipal = new(claimsIdentity);

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            await context.Response.WriteAsJsonAsync(new { message = "user is logged in" });
        }

        private static Task<IResult> HandleRegister()
        {
            return Task.FromResult(Results.Ok("user just signed in"));
        }

        private static async void HandleLogout(HttpContext context)
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await context.Response.WriteAsJsonAsync(new { message = "user successfully logged out" });
        }
    }
}
