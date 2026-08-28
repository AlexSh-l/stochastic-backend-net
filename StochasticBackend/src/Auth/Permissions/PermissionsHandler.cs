using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StochasticBackend.src.Auth.Configuration;
using StochasticBackend.src.Shared.DatabasePSQL;
using System.Security.Claims;

namespace StochasticBackend.src.Auth.Permissions
{
    public class PermissionsHandler(IMemoryCache memoryCache, IServiceScopeFactory scopeFactory): AuthorizationHandler<PermissionsRequirement>
    {
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;


        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionsRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true) return;

            var role = context.User.FindFirst(ClaimsIdentity.DefaultRoleClaimType)?.Value;
            var userId = context.User.FindFirst(ClaimsIdentity.DefaultNameClaimType)?.Value;
            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userId)) return;

            string cacheKey = $"user_permissions_{userId}";
            var userPermissions = await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);

                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                return await dbContext.Permissions
                .Where(p => p.Roles
                    .Any(r => r.Name == role)
                ).Select(p => p.Name.ToLower())
                .ToListAsync();
            }) ?? new List<string>();

            if (requirement.Operator == EPermissionOperator.And)
            {
                if (requirement.Permissions.All(p => userPermissions.Contains(p.ToLower())))
                {
                    context.Succeed(requirement);
                }
            }
            else if (requirement.Operator == EPermissionOperator.Or)
            {
                if (requirement.Permissions.Any(p => userPermissions.Contains(p.ToLower())))
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
