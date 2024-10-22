using AttendifyClientProjectC;
using AttendifyClientProjectC.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;

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
    options.AddPolicy("IsAllowedPolicy", policy =>
        policy.Requirements.Add(new IsAllowedRequirement(true))); 
}); // this works! huzzah

builder.Services.AddSingleton<IAuthorizationHandler, IsAllowedHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider,DefaultAuthorizationPolicyProvider>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<EmailService>();



await builder.Build().RunAsync();
