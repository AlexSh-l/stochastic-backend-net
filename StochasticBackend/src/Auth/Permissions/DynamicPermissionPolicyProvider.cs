using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using StochasticBackend.src.Auth.Configuration;

namespace StochasticBackend.src.Auth.Permissions
{
    // Authorize attribute string convention: "Permission|{Operator}|{Permission1:Action1},{Permission2:Action2},..."
    public class DynamicPermissionPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
    {
        public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; } = new(options);

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => FallbackPolicyProvider.GetFallbackPolicyAsync();

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(PermissionNameConfig.permissionNameHeader, StringComparison.OrdinalIgnoreCase))
            {
                string[] namePayload = policyName.Split(PermissionNameConfig.permissionNameDivider);
                string payloadOperator = namePayload[1];
                string payloadPermissions = namePayload[2];

                EPermissionOperator permissionOperator = Enum.TryParse<EPermissionOperator>(payloadOperator, true, out var op) ? op : EPermissionOperator.And;

                string[] permissions = payloadPermissions.Split(PermissionNameConfig.permissionValuesDivider);

                if (permissions.Length > 0)
                {
                    var policy = new AuthorizationPolicyBuilder();
                    var requirement = new PermissionsRequirement(permissions, permissionOperator);
                    policy.AddRequirements(requirement);
                    return Task.FromResult<AuthorizationPolicy?>(policy.Build());
                }
            }

            return FallbackPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}
