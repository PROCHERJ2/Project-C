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

                return new AuthenticationState(user);
            }

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }
}
