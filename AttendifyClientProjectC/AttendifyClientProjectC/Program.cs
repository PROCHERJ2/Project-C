using AttendifyClientProjectC;
using AttendifyClientProjectC.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddBlazoredLocalStorage();

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//Dont forget to put services here!
builder.Services.AddScoped<TestService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
//builder.Services.AddAuthorizationCore();
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("IsAllowedPolicy", policy =>                                  //just for testing
        policy.Requirements.Add(new IsAllowedRequirement(true)));
    options.AddPolicy("IsAdminPolicy", policy =>
    {
        policy.Requirements.Add(new CustomRoleRequirement()); 
    });
}); // this works! huzzah

builder.Services.AddSingleton<IAuthorizationHandler, IsAllowedHandler>();           //also just for testing. dont forget to remove this later, me 
builder.Services.AddScoped<IAuthorizationHandler, RoleBasedAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider,DefaultAuthorizationPolicyProvider>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<AdminUserService>();
builder.Services.AddScoped<AdminEventService>();

await builder.Build().RunAsync();
