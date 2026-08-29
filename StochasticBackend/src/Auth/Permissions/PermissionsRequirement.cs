using Microsoft.AspNetCore.Authorization;
using StochasticBackend.src.Auth.Configuration;

namespace StochasticBackend.src.Auth.Permissions
{
    public class PermissionsRequirement(string[] permissions, EPermissionOperator permissionOperator): IAuthorizationRequirement
    {
        public string[] Permissions { get; } = permissions;
        public EPermissionOperator Operator { get; } = permissionOperator;
    }
}
