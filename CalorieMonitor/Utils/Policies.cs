using CalorieMonitor.Core.Enums;
using Microsoft.AspNetCore.Authorization;

namespace CalorieMonitor.Utils
{
    public class Policies
    {
        public const string ManageUsers = "ManageUsers";
        public static AuthorizationPolicy ManageUsersPolicy()
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole(UserRole.Admin.ToString(),
                UserRole.UserManager.ToString())
                .Build();
        }
    }
}
