namespace StochasticBackend.src.Auth.Configuration
{
    public static class RolesToPermissionsMap
    {
        public static Dictionary<string, string[]> RolesMap = new Dictionary<string, string[]>
        {
            {UserRoles.REGULAR, [ UserPermissions.VIEW_IMAGES ] },
            {UserRoles.ADMIN, [ UserPermissions.VIEW_IMAGES, UserPermissions.EDIT_IMAGES ]},
        };
    }
}
