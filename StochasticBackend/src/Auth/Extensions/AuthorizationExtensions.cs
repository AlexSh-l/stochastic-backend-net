using Microsoft.AspNetCore.Authorization;
using StochasticBackend.src.Auth.Configuration;
using StochasticBackend.src.Auth.Permissions;

namespace StochasticBackend.src.Auth.Extensions
{
    public static class AuthorizationExtensions
    {
        public static RouteHandlerBuilder RequirePermissions(
            this RouteHandlerBuilder builder,
            EPermissionOperator permissionOperator,
            params string[] permissions)
        {
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionsRequirement(permissions, permissionOperator))
                .Build();

            return builder.RequireAuthorization(policy);
        }
    }
}
