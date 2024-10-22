namespace AttendifyClientProjectC
{
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;

    public class IsAllowedRequirement : IAuthorizationRequirement
    {
        public bool IsAllowed { get; }

        public IsAllowedRequirement(bool isAllowed)
        {
            IsAllowed = isAllowed;
        }
    }

    public class IsAllowedHandler : AuthorizationHandler<IsAllowedRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsAllowedRequirement requirement)
        {
            if (requirement.IsAllowed) //if true = access yes
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
