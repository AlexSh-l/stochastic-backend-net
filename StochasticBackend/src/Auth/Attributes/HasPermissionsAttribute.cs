using Microsoft.AspNetCore.Authorization;
using StochasticBackend.src.Auth.Configuration;

namespace StochasticBackend.src.Auth.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class HasPermissionsAttribute: AuthorizeAttribute
    {
        public HasPermissionsAttribute(EPermissionOperator permissionOperator, params string[] permissions)
        {
            Policy = $"{PermissionNameConfig.permissionNameHeader}{PermissionNameConfig.permissionNameDivider}{permissionOperator}{PermissionNameConfig.permissionNameDivider}{string.Join(PermissionNameConfig.permissionValuesDivider, permissions)}";
        }
    }
}
