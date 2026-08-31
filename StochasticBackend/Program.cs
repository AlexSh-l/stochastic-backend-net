using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using stochastic_backend_net.src.Shared.Extensions;
using StochasticBackend.src.Auth.Permissions;
using StochasticBackend.src.Shared.DatabasePSQL;
using StochasticBackend.src.Shared.Exceptions;
using StochasticBackend.src.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

var dbConnectionString = builder.Configuration.GetConnectionString("PostgresConnection");

builder.Services.AddDbContext<ApplicationContext>((options) => {
    Console.WriteLine(dbConnectionString);
    options.UseNpgsql(dbConnectionString);
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
    });
builder.Services.AddAuthorization();

builder.Services.AddSingleton<IAuthorizationPolicyProvider, DynamicPermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionsHandler>();

builder.Services.AddMemoryCache();

builder.Services.AddSingletonServices();
builder.Services.AddScopedServices();

var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();
