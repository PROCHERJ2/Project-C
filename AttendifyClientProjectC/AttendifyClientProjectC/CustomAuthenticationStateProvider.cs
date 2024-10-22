using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace AttendifyClientProjectC
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly Services.AuthenticationService _authService;

        public CustomAuthenticationStateProvider(HttpClient httpClient, Services.AuthenticationService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        public void NotifyAuthenticationStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            await _authService.SetAuthorizationHeaderAsync();
            var response = await _httpClient.GetAsync("https://localhost:7059/api/authentication/user");

            if (response.IsSuccessStatusCode)
            {
                var userJson = await response.Content.ReadAsStringAsync();
                var claimsDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(userJson);
                var claims = claimsDictionary.Select(kvp => new Claim(kvp.Key, kvp.Value)).ToList();
                var identity = new ClaimsIdentity(claims, "Bearer");
                var user = new ClaimsPrincipal(identity);

                // Store the role in local storage for later access -> but to access pages where you need to be a teacher, admin or super
                // admin we need to do a check to the server and then store a secure key itself. so this is fine for students ( which will
                // represent the majority of our users, so its better performance wise if they won't do a server request every time they need
                // to access certain pages ), but for teachers and admin only pages there needs to be another stopgap, and imho this is kinda
                // obselete because we can just do the default authorize / unauthorize for the student pages, and the other roles can do what the
                // students can.

                var role = claimsDictionary["role"];
                await _authService.StoreUserRoleAsync(role); 

                return new AuthenticationState(user);
            }

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

    }
}
