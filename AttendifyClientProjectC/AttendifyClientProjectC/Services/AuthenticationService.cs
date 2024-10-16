using AttendifySharedProjectC.Models;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace AttendifyClientProjectC.Services
{
    public class AuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public AuthenticationService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        //login stuff
        public async Task SetAuthorizationHeaderAsync()
        {
            var accessToken = await _localStorage.GetItemAsStringAsync("accessToken");
            if (!string.IsNullOrEmpty(accessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("accessToken");
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        //login stuff

        //email stuff
        public async Task<string> GenerateVerificationTokenAsync()
        {
            //var response = await _httpClient.PostAsync("api/authentication/generate-verification-token", null);
            var response = await _httpClient.PostAsync("https://localhost:7059/api/authentication/generate-verification-token", null);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            throw new Exception("Failed to generate verification token.");
        }

        public async Task<string> VerifyTokenAsync(string token)
        {
            //var response = await _httpClient.PostAsJsonAsync("api/authentication/verify-token", token);
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7059/api/authentication/verify-token", token);
            return await response.Content.ReadAsStringAsync();
        }
        //email stuff


        public async Task CreateUserAsync(RegistrationModel newUser)
        {
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7059/api/authentication/create-user", newUser);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("User created successfully.");
            }
            else
            {
                Console.WriteLine("Failed to create user.");
            }
        }
    }
}
