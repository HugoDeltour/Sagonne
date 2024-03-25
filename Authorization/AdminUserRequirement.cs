using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Sagonne.DataBase.Table;
using System.Security.Claims;

namespace Sagonne.AuthorizationRequirements
{
    public class AdminUserRequirement : IAuthorizationRequirement
    {
    }

    public class AdminUserHandler : AuthorizationHandler<AdminUserRequirement>
    {

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminUserRequirement requirement)
        {
            var AdminClaim = context.User.Identities.FirstOrDefault().Claims.First(c=> c.Type == ClaimTypes.Role);
            if (AdminClaim.Value == "ADMIN")
            {
                context.Succeed(requirement);
            }
        }

    }
}
