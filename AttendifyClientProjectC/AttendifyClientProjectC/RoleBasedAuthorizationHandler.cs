namespace AttendifyClientProjectC
{
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;
    using Blazored.LocalStorage;

    public class RoleBasedAuthorizationHandler : AuthorizationHandler<IAuthorizationRequirement>
    {
        private readonly ILocalStorageService _localStorage;

        public RoleBasedAuthorizationHandler(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var userRole = await _localStorage.GetItemAsStringAsync("userRole");                    //this is just temporary. Because role being saved
            Console.WriteLine($"Userrole on requirements check:{userRole}");                        //in the browser represents a security risk.
            if (userRole == "Admin" || userRole == "SuperAdmin")                                    //thats no big deal for students, who won't be
            {                                                                                       //able to do much, but if people set their role to
                context.Succeed(requirement);                                                       //admin or teacher then that would be bad. 
            }                                                                                       //this can be fixed by basically requesting the 
                                                                                                    //role from the server instead which would make
            return;                                                                                 //it more secure. but for now, this is okay
        }
    }
    public class CustomRoleRequirement : IAuthorizationRequirement
    {
        // marker for policy, leave it empty ( dont worry about it, its empty for a reason. )
    }
}
