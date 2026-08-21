using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StochasticBackend.src.Auth.Configuration;

namespace StochasticBackend.src.Auth.Permissions
{
    public class PermissionsHandler(IMemoryCache memoryCache, IServiceScopeFactory scopeFactory): AuthorizationHandler<PermissionsRequirement>
    {
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;


        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionsRequirement requirement)
        {
            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return;

            //if (context.User?.Identity?.IsAuthenticated != true)
            //{
            //    return Task.CompletedTask;
            //}

            string cacheKey = $"user_permissions_{userIdClaim}";
            //var userPermissions = await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);

            //    using var scope = _scopeFactory.CreateScope();
                //var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                //return await dbContext.UserPermissions
                //.Where(up => up.UserId == userIdClaim)
                //.Select(up => up.PermissionName)
                //.ToListAsync();
            //}) ?? new List<string>();

            var userPermissions = new List<string>();

            // // Extract all permission claim values from the current user
            // //var userPermissions = context.User.FindAll("permission").Select(c => c.Value).ToHashSet();

            if (requirement.Operator == EPermissionOperator.And)
            {
                // All requested permissions must exist in the user's claims
                if (requirement.Permissions.All(p => userPermissions.Contains(p)))
                {
                    context.Succeed(requirement);
                }
            }
            else if (requirement.Operator == EPermissionOperator.Or)
            {
                // At least one requested permission must exist in the user's claims
                if (requirement.Permissions.Any(p => userPermissions.Contains(p)))
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
