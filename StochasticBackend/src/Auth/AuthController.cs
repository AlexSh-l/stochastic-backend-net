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
        //private static List<User> users = new List<User> {
        //    new User { Login = "TestUser1", Password = "TestUser12345", Role = new Role() },
        //    new User { Login = "TestUser2", Password = "TestUser23456", Role = new Role() }
        //};

        public static void MapEndpoint(IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/auth").WithTags("Auth");
            group.MapGet("/login", HandleLogin);
            group.MapGet("/register", HandleRegister);
            group.MapGet("/logout", HandleLogout);
        }

        private static async Task HandleLogin(HttpContext context, ApplicationContext dbContext, IAuthService authService)
        {
            string login = "TestUser1";
            string password = "TestUserPassword1";

            UserDTO? user = await authService.LogUserInAsync(login, password);
            //User? user = users.FirstOrDefault(u => u.Login == login);
            if (user is null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Unable to find user" });
                return;
            }

            var claims = new List<Claim> { 
                new(ClaimTypes.Name, user.Login),
                new(ClaimTypes.Role, user.Role.Name)
            };
            ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal claimsPrincipal = new(claimsIdentity);

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            //using(dbContext)
            //{
            //var permissions = dbContext.Permissions.ToList();
            //foreach (var permission in permissions)
            //{
            //    Console.WriteLine(permission.Name);
            //}
            //}


            //var role = dbContext.Roles.FirstOrDefault(value => value.Id == 3);
            //var permission1 = dbContext.Permissions.FirstOrDefault(value => value.Id == 2);
            //if (permission1 is not null && role is not null)
            //{
            //    role.Permissions.Add(permission1);
            //}

            //dbContext.SaveChanges();


            await context.Response.WriteAsJsonAsync(new { message = "user is logged in" });
        }

        private static async Task<IResult> HandleRegister(HttpContext context, ApplicationContext dbContext, IAuthService authService)
        {
            string login = "TestUser1";
            string password = "TestUserPassword1";

            UserDTO? user = await authService.RegisterUserAsync(login, password);
            if (user is null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Unable to register user" });
                return Results.BadRequest();
            }

            var claims = new List<Claim> {
                new(ClaimTypes.Name, user.Login),
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
